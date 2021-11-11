#pragma once

#include "WebRTCPlugin.h"

namespace unity
{
namespace webrtc
{
    using namespace ::webrtc;

    class AudioDeviceWrapper : public webrtc::AudioDeviceModule
    {
    public:
        AudioDeviceWrapper();
        virtual ~AudioDeviceWrapper() = default;

        int32 ActiveAudioLayer(AudioLayer* audioLayer) const override { return adm_->ActiveAudioLayer(audioLayer); }
        int32 RegisterAudioCallback(webrtc::AudioTransport* transport) override { return adm_->RegisterAudioCallback(transport); }
        int32 Init() override { return adm_->Init(); }
        int32 Terminate() override { return adm_->Terminate(); }
        bool Initialized() const override { return adm_->Initialized(); }
        int16 PlayoutDevices() override { return adm_->PlayoutDevices(); }
        int16 RecordingDevices() override { return adm_->RecordingDevices(); }
        int32 PlayoutDeviceName(uint16 index,
            char name[webrtc::kAdmMaxDeviceNameSize],
            char guid[webrtc::kAdmMaxGuidSize]) override {
                return adm_->PlayoutDeviceName(index, name, guid);
        }
        int32 RecordingDeviceName(uint16 index,
            char name[webrtc::kAdmMaxDeviceNameSize],
            char guid[webrtc::kAdmMaxGuidSize]) override {
                return adm_->RecordingDeviceName(index, name, guid);
        }
        int32 SetPlayoutDevice(uint16 index) override { return adm_->SetPlayoutDevice(index); }
        int32 SetPlayoutDevice(WindowsDeviceType device) override { return adm_->SetPlayoutDevice(device); }
        int32 SetRecordingDevice(uint16 index) override { return adm_->SetRecordingDevice(index); }
        int32 SetRecordingDevice(WindowsDeviceType device) override { return adm_->SetRecordingDevice(device); }
        int32 PlayoutIsAvailable(bool* available) override { return adm_->PlayoutIsAvailable(available);}
        int32 InitPlayout() override { return adm_->InitPlayout(); }
        bool PlayoutIsInitialized() const override { return adm_->PlayoutIsInitialized(); }
        int32 RecordingIsAvailable(bool* available) override { return adm_->RecordingIsAvailable(available); }
        int32 InitRecording() override;
        bool RecordingIsInitialized() const override { return adm_->RecordingIsInitialized(); }
        int32 StartPlayout() override { return adm_->StartPlayout(); }
        int32 StopPlayout() override { return adm_->StopPlayout(); }
        bool Playing() const override { return adm_->Playing(); }
        int32 StartRecording() override { return adm_->StartRecording(); }
        int32 StopRecording() override { return adm_->StopRecording(); }
        bool Recording() const override { return adm_->Recording(); }
        int32 InitSpeaker() override { return adm_->InitSpeaker(); }
        bool SpeakerIsInitialized() const override { return adm_->SpeakerIsInitialized(); }
        int32 InitMicrophone() override { return adm_->InitMicrophone(); }
        bool MicrophoneIsInitialized() const override { return adm_->MicrophoneIsInitialized(); }
        int32 SpeakerVolumeIsAvailable(bool* available) override { return adm_->SpeakerVolumeIsAvailable(available); }
        int32 SetSpeakerVolume(uint32 volume) override { return adm_->SetSpeakerVolume(volume); }
        int32 SpeakerVolume(uint32* volume) const override { return adm_->SpeakerVolume(volume); }
        int32 MaxSpeakerVolume(uint32* maxVolume) const override { return adm_->MaxSpeakerVolume(maxVolume); }
        int32 MinSpeakerVolume(uint32* minVolume) const override { return adm_->MinSpeakerVolume(minVolume); }
        int32 MicrophoneVolumeIsAvailable(bool* available) override { return adm_->MicrophoneVolumeIsAvailable(available); }
        int32 SetMicrophoneVolume(uint32 volume) override { return adm_->SetMicrophoneVolume(volume); }
        int32 MicrophoneVolume(uint32* volume) const override { return adm_->MicrophoneVolume(volume); }
        int32 MaxMicrophoneVolume(uint32* maxVolume) const override { return adm_->MaxMicrophoneVolume(maxVolume); }
        int32 MinMicrophoneVolume(uint32* minVolume) const override { return adm_->MinMicrophoneVolume(minVolume); }
        int32 SpeakerMuteIsAvailable(bool* available) override { return adm_->SpeakerMuteIsAvailable(available); }
        int32 SetSpeakerMute(bool enable) override { return adm_->SetSpeakerMute(enable); }
        int32 SpeakerMute(bool* enabled) const override { return adm_->SpeakerMute(enabled); }
        int32 MicrophoneMuteIsAvailable(bool* available) override { return adm_->MicrophoneMuteIsAvailable(available); }
        int32 SetMicrophoneMute(bool enable) override { return adm_->SetMicrophoneMute(enable); }
        int32 MicrophoneMute(bool* enabled) const override { return adm_->MicrophoneMute(enabled); }
        int32 StereoPlayoutIsAvailable(bool* available) const override { return adm_->StereoPlayoutIsAvailable(available); }
        int32 SetStereoPlayout(bool enable) override { return adm_->SetStereoPlayout(enable); }
        int32 StereoPlayout(bool* enabled) const override { return adm_->StereoPlayout(enabled); }
        int32 StereoRecordingIsAvailable(bool* available) const override { return adm_->StereoRecordingIsAvailable(available); }
        int32 SetStereoRecording(bool enable) override { return adm_->SetStereoRecording(enable); }
        int32 StereoRecording(bool* enabled) const override { return adm_->StereoRecording(enabled); }
        int32 PlayoutDelay(uint16* delayMS) const override { return adm_->PlayoutDelay(delayMS); }
        bool BuiltInAECIsAvailable() const override { return adm_->BuiltInAECIsAvailable(); }
        bool BuiltInAGCIsAvailable() const override { return adm_->BuiltInAGCIsAvailable(); }
        bool BuiltInNSIsAvailable() const override { return adm_->BuiltInNSIsAvailable(); }
        int32 EnableBuiltInAEC(bool enable) override { return adm_->EnableBuiltInAEC(enable); }
        int32 EnableBuiltInAGC(bool enable) override { return adm_->EnableBuiltInAGC(enable); }
        int32 EnableBuiltInNS(bool enable) override { return adm_->EnableBuiltInNS(enable); }
#if defined(WEBRTC_IOS)
        virtual int GetPlayoutAudioParameters(webrtc::AudioParameters* params) const override { return adm_->GetPlayoutAudioParameters(params); }
        virtual int GetRecordAudioParameters(webrtc::AudioParameters* params) const override { return adm_->GetRecordAudioParameters(params); }
#endif

        void SetSelectedMicIndex(int index) { mic_index_ = index; }

    private:
        rtc::scoped_refptr<AudioDeviceModule> adm_;
        int mic_index_ = 0;
    };

} // end namespace webrtc
} // end namespace unity
