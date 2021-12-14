using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.WebRTC
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="renderer"></param>
    public delegate void OnAudioReceived(AudioClip renderer);

    /// <summary>
    ///
    /// </summary>
    public class AudioStreamTrack : MediaStreamTrack
    {
        /// <summary>
        ///
        /// </summary>
        public event OnAudioReceived OnAudioReceived;

        /// <summary>
        ///
        /// </summary>
        public AudioClip Renderer
        {
            get { return _streamRenderer?.clip; }
        }

        internal class AudioBufferTracker
        {
            public const int NumOfFramesForBuffering = 5;
            public long BufferPosition { get; set; }
            public int SamplesPer10ms { get { return m_samplesPer10ms; } }

            private readonly int m_sampleLength;
            private readonly int m_samplesPer10ms;
            private readonly int m_samplesForBuffering;
            private long m_renderPos;
            private int m_prevTimeSamples;

            public AudioBufferTracker(int sampleLength)
            {
                m_sampleLength = sampleLength;
                m_samplesPer10ms = m_sampleLength / 100;
                m_samplesForBuffering = m_samplesPer10ms * NumOfFramesForBuffering;
            }

            public void Initialize(AudioSource source)
            {
                var timeSamples = source.timeSamples;
                m_prevTimeSamples = timeSamples;
                m_renderPos = timeSamples;
                BufferPosition = timeSamples;
            }

            public int CheckNeedCorrection(AudioSource source)
            {
                if (source != null && m_prevTimeSamples != source.timeSamples)
                {
                    var timeSamples = source.timeSamples;
                    m_renderPos += (timeSamples < m_prevTimeSamples ? m_sampleLength : 0) + timeSamples - m_prevTimeSamples;
                    m_prevTimeSamples = timeSamples;

                    if (m_renderPos >= BufferPosition)
                    {
                        return (int)(m_renderPos - BufferPosition) + m_samplesForBuffering;
                    }
                    else if (BufferPosition - m_renderPos <= m_samplesPer10ms)
                    {
                        return (int)(m_renderPos + m_samplesForBuffering - BufferPosition);
                    }
                }

                return 0;
            }
        }


        internal class AudioStreamRenderer : IDisposable
        {
            private AudioClip m_clip;
            private bool m_bufferReady = false;
            private readonly Queue<float[]> m_recvBufs = new Queue<float[]>();
            private readonly AudioBufferTracker m_bufInfo;
            private AudioSource m_attachedSource;

            public AudioClip clip
            {
                get
                {
                    return m_clip;
                }
            }

            public AudioStreamRenderer(string name, int sampleRate, int channels)
            {
                int lengthSamples = sampleRate;  // sample length for 1 second

                // note:: OnSendAudio and OnAudioSetPosition callback is called before complete the constructor.
                m_clip = AudioClip.Create($"{name}-{GetHashCode():x}", lengthSamples, channels, sampleRate, false);
                m_bufInfo = new AudioBufferTracker(sampleRate);

                Debug.Log($"AudioClip is generated: {sampleRate}, {channels}");

            }

            public void Dispose()
            {
                if (m_clip != null)
                {
                    WebRTC.DestroyOnMainThread(m_clip);
                }
                m_clip = null;
                m_recvBufs.Clear();
            }

            internal AudioSource FindAttachedAudioSource()
            {
                foreach (var audioSource in GameObject.FindObjectsOfType<AudioSource>())
                {
                    if (audioSource.clip != null && audioSource.clip.name == m_clip.name)
                    {
                        return audioSource;
                    }
                }
                return null;
            }

            internal void WriteToAudioClip(int numOfFrames = 1)
            {
                int baseOffset = (int)(m_bufInfo.BufferPosition % m_clip.samples);
                int writtenSamples = 0;

                while (numOfFrames-- > 0)
                {
                    writtenSamples += WriteBuffer(
                        m_recvBufs.Count > 0 ? m_recvBufs.Dequeue() : null,
                        baseOffset + writtenSamples);
                }

                m_bufInfo.BufferPosition += writtenSamples;

                int WriteBuffer(float[] data, int offset)
                {
                    data ??= new float[m_bufInfo.SamplesPer10ms * m_clip.channels];
                    m_clip.SetData(data, offset % m_clip.samples);
                    return data.Length / m_clip.channels;
                }
            }

            internal void SetData(float[] data)
            {

                m_recvBufs.Enqueue(data);
                if (m_clip == null)
                {
                    return;
                }

                if (m_recvBufs.Count >= AudioBufferTracker.NumOfFramesForBuffering && m_bufferReady == false)
                {
                    var audioSource = FindAttachedAudioSource();
                    if (audioSource)
                    {
                        m_attachedSource = audioSource;
                        m_bufInfo.Initialize(m_attachedSource);
                    }

                    WriteToAudioClip(AudioBufferTracker.NumOfFramesForBuffering - 1);
                    m_bufferReady = true;
                }

                if (m_bufferReady)
                {
                    int correctSize = m_bufInfo.CheckNeedCorrection(m_attachedSource);
                    if (correctSize > 0)
                    {
                        Debug.Log($"Audio buffer correction : {correctSize}");
                        WriteToAudioClip(correctSize / m_bufInfo.SamplesPer10ms +
                            ((correctSize % m_bufInfo.SamplesPer10ms) > 0 ? 1 : 0));
                    }
                    else
                    {
                        WriteToAudioClip();
                    }
                }
            }
        }

        
        /// <summary>
        /// The channel count of streaming receiving audio is changing at the first few frames.
        /// So This count is for ignoring the unstable audio frames
        /// </summary>
        const int MaxFrameCountReceiveDataForIgnoring = 5;

        AudioStreamRenderer _streamRenderer;
        AudioTrackSource _source;

        int frameCountReceiveDataForIgnoring = 0; 

        /// <summary>
        ///
        /// </summary>
        public AudioStreamTrack(bool noiseSuppress, bool autoGainCtrl, bool highPassFilter)
            : this(Guid.NewGuid().ToString(), new AudioTrackSource(), noiseSuppress, autoGainCtrl, highPassFilter)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        [Obsolete("Support for AudioSource is deprecated.")]
        public AudioStreamTrack(AudioSource source, bool noiseSuppress = true, bool autoGainCtrl = true, bool highPassFilter = true)
            : this(noiseSuppress, autoGainCtrl, highPassFilter)
        {
            throw new NotSupportedException();
        }

        internal AudioStreamTrack(string label, AudioTrackSource source, bool noiseSuppress, bool autoGainCtrl, bool highPassFilter)
            : this(WebRTC.Context.CreateAudioTrack(label, source.self, noiseSuppress, autoGainCtrl, highPassFilter))
        {
            _source = source;
        }

        internal AudioStreamTrack(IntPtr ptr) : base(ptr)
        {
            WebRTC.Context.AudioTrackRegisterAudioReceiveCallback(self, OnAudioReceive);
        }

        /// <summary>
        ///
        /// </summary>
        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                _streamRenderer?.Dispose();
                _source?.Dispose();
                WebRTC.Context.AudioTrackUnregisterAudioReceiveCallback(self);
            }
            base.Dispose();
        }

        private void OnAudioReceivedInternal(float[] audioData, int sampleRate, int channels, int numOfFrames)
        {
            if (_streamRenderer == null)
            {
                if(frameCountReceiveDataForIgnoring < MaxFrameCountReceiveDataForIgnoring)
                {
                    frameCountReceiveDataForIgnoring++;
                    return;
                }
                // TODO(jeonghun): 싱글 채널로 오디오가 들어올 때의 workaround, 임시 대응
                // 현재로선 원인은 알 수 없지만 스테레오 오디오가 싱글 채널인 것 처럼 들어올 때가 있음
                // 오디오 데이터는 스테레오 오디오 그대로, 실제 호출 주기도 10ms 마다 2번 연속 호출되고 있음
                // 그래서 해당 경우 강제로 stereo 오디오로 처리, 정확한 원인을 파악하여 수정이 필요
                // 재현 방법: 아주 높은 확율로 3번째 멤버가 룸 입장 시 발생.
                if (channels == 1)
                {
                    Debug.Log($"Force treat audio as stereo (original channel count:{channels})");
                    channels = 2;
                }
                _streamRenderer = new AudioStreamRenderer(this.Id, sampleRate, channels);

                OnAudioReceived?.Invoke(_streamRenderer.clip);
            }
            _streamRenderer?.SetData(audioData);
        }

        [AOT.MonoPInvokeCallback(typeof(DelegateAudioReceive))]
        static void OnAudioReceive(
            IntPtr ptrTrack, float[] audioData, int size, int sampleRate, int numOfChannels, int numOfFrames)
        {
            WebRTC.Sync(ptrTrack, () =>
            {
                if (WebRTC.Table[ptrTrack] is AudioStreamTrack track)
                {
                    track.OnAudioReceivedInternal(audioData, sampleRate, numOfChannels, numOfFrames);
                }
            });
        }
    }
    internal class AudioTrackSource : RefCountedObject
    {
        public AudioTrackSource() : base(WebRTC.Context.CreateAudioTrackSource())
        {
            WebRTC.Table.Add(self, this);
        }

        ~AudioTrackSource()
        {
            this.Dispose();
        }

        public override void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            if (self != IntPtr.Zero && !WebRTC.Context.IsNull)
            {
                WebRTC.Table.Remove(self);
            }
            base.Dispose();
        }
    }
}
