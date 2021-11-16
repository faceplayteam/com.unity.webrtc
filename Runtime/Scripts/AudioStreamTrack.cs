using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
        public AudioSource Source { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public AudioClip Renderer
        {
            get { return _streamRenderer?.clip; }
        }


        internal class AudioStreamRenderer : IDisposable
        {
            private AudioClip m_clip;
            private bool m_bufferReady = false;
            private int m_offset = 0;
            private readonly Queue<float[]> m_recvBufs = new Queue<float[]>();
            private readonly int m_samplesPer10ms;
            private AudioSource m_attachedSource;

            private const int BufferingCount = 10;

            internal enum BufferState
            {
                Unknown,
                Inactive,
                Normal,
                WarningOverrun,
                Overrun,
            }

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
                m_clip = AudioClip.Create(name + GetHashCode(), lengthSamples, channels, sampleRate, false);
                m_samplesPer10ms = sampleRate / 100;
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
                    if (audioSource.clip.name == m_clip.name)
                    {
                        return audioSource;
                    }
                }
                return null;
            }

            internal void WriteToAudioClip(int cnt = 1, bool force = false)
            {
                while (cnt-- > 0 && m_recvBufs.Count > 0)
                {
                    WriteBuffer(m_recvBufs.Dequeue());
                }
                if (force)
                {
                    while (cnt-- > 0)
                    {
                        WriteBuffer(new float[m_samplesPer10ms * m_clip.channels]);
                    }
                }

                void WriteBuffer(float[] data)
                {
                    m_clip.SetData(data, m_offset);
                    m_offset = (m_offset + (data.Length / m_clip.channels)) % m_clip.samples;
                }
            }

            internal BufferState CheckBufferState()
            {
                if (m_attachedSource != null)
                {
                    if (m_attachedSource.isPlaying == false)
                    {
                        return BufferState.Inactive;
                    }

                    if (m_attachedSource.timeSamples < m_offset)
                    {
                        if (m_offset - m_attachedSource.timeSamples <= m_samplesPer10ms)
                        {
                            return BufferState.WarningOverrun;
                        }
                    }
                    else if (m_attachedSource.timeSamples >= m_offset)
                    {
                        bool checkBufferWrap = m_offset < m_clip.samples * 0.2 &&
                            m_clip.samples * 0.8 < m_attachedSource.timeSamples;
                        if (checkBufferWrap == false || m_attachedSource.timeSamples == m_offset)
                        {
                            return BufferState.Overrun;
                        }
                    }

                    return BufferState.Normal;
                }
                return BufferState.Unknown;
            }

            internal void SetData(float[] data)
            {
                m_recvBufs.Enqueue(data);

                if (m_recvBufs.Count > BufferingCount && m_bufferReady == false)
                {
                    var audioSource = FindAttachedAudioSource();
                    if (audioSource)
                    {
                        m_offset = audioSource.timeSamples;
                        m_attachedSource = audioSource;
                    }

                    WriteToAudioClip(BufferingCount / 2);
                    m_bufferReady = true;
                }

                if (m_bufferReady)
                {
                    switch (CheckBufferState())
                    {
                        case BufferState.Inactive:
                            break;

                        case BufferState.WarningOverrun:
                            Debug.Log("BufferState.WarningOverrun");
                            WriteToAudioClip(BufferingCount / 3, true);
                            break;

                        case BufferState.Overrun:
                            Debug.Log("BufferState.Overrun");
                            m_offset = m_attachedSource.timeSamples;
                            WriteToAudioClip(BufferingCount / 2, true);
                            break;

                        case BufferState.Normal:
                        case BufferState.Unknown:
                            WriteToAudioClip(); ;
                            break;
                    }
                }
            }
        }

        
        /// <summary>
        /// The channel count of streaming receiving audio is changing at the first few frames.
        /// So This count is for ignoring the unstable audio frames
        /// </summary>
        const int MaxFrameCountReceiveDataForIgnoring = 5;

        readonly AudioSourceRead _audioSourceRead;
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
        public AudioStreamTrack(AudioSource source, bool noiseSuppress = true, bool autoGainCtrl = true, bool highPassFilter = true)
            : this(noiseSuppress, autoGainCtrl, highPassFilter)
        {
            if (source == null)
                throw new ArgumentNullException("AudioSource argument is null");
            if (source.clip == null)
                throw new ArgumentException("AudioClip must to be attached on AudioSource");
            Source = source;

            _audioSourceRead = source.gameObject.AddComponent<AudioSourceRead>();
            _audioSourceRead.hideFlags = HideFlags.HideInHierarchy;
            _audioSourceRead.onAudioRead += SetData;
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
                if (_audioSourceRead != null)
                {
                    // Unity API must be called from main thread.
                    _audioSourceRead.onAudioRead -= SetData;
                    WebRTC.DestroyOnMainThread(_audioSourceRead);
                }
                _streamRenderer?.Dispose();
                _source?.Dispose();
                WebRTC.Context.AudioTrackUnregisterAudioReceiveCallback(self);
            }
            base.Dispose();
        }

#if UNITY_2020_1_OR_NEWER
        /// <summary>
        ///
        /// </summary>
        /// <param name="nativeArray"></param>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        public void SetData(ref NativeArray<float>.ReadOnly nativeArray, int channels, int sampleRate)
        {
            unsafe
            {
                void* ptr = nativeArray.GetUnsafeReadOnlyPtr();
                ProcessAudio(GetSelfOrThrow(), (IntPtr)ptr, sampleRate, channels, nativeArray.Length);
            }
        }
#endif

        /// <summary>
        ///
        /// </summary>
        /// <param name="nativeArray"></param>
        /// <param name="channels"></param>
        /// <param name="sampleRate"></param>
        public void SetData(ref NativeArray<float> nativeArray, int channels, int sampleRate)
        {
            unsafe
            {
                void* ptr = nativeArray.GetUnsafeReadOnlyPtr();
                ProcessAudio(GetSelfOrThrow(), (IntPtr)ptr, sampleRate, channels, nativeArray.Length);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="nativeSlice"></param>
        /// <param name="channels"></param>
        public void SetData(ref NativeSlice<float> nativeSlice, int channels, int sampleRate)
        {
            unsafe
            {
                void* ptr = nativeSlice.GetUnsafeReadOnlyPtr();
                ProcessAudio(GetSelfOrThrow(), (IntPtr)ptr, sampleRate, channels, nativeSlice.Length);
            }
        }

        static void ProcessAudio(IntPtr track, IntPtr array, int sampleRate, int channels, int frames)
        {
            if (sampleRate == 0 || channels == 0 || frames == 0)
                throw new ArgumentException($"arguments are invalid values " +
                    $"sampleRate={sampleRate}, " +
                    $"channels={channels}, " +
                    $"frames={frames}");
            WebRTC.Context.ProcessLocalAudio(track, array, sampleRate, channels, frames);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="array"></param>
        /// <param name="channels"></param>
        public void SetData(float[] array, int channels, int sampleRate)
        {
            if (array == null)
                throw new ArgumentNullException("array is null");
            NativeArray<float> nativeArray = new NativeArray<float>(array, Allocator.Temp);
            SetData(ref nativeArray, channels, sampleRate);
            nativeArray.Dispose();
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
