#pragma once

#include <cstdint>
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

        int32_t ActiveAudioLayer(AudioLayer* audioLayer) const override { return adm_->ActiveAudioLayer(audioLayer); }
        int32_t RegisterAudioCallback(webrtc::AudioTransport* transport) override { return adm_->RegisterAudioCallback(transport); }
        int32_t Init() override { return adm_->Init(); }
        int32_t Terminate() override { return adm_->Terminate(); }
        bool Initialized() const override { return adm_->Initialized(); }
        int16_t PlayoutDevices() override { return adm_->PlayoutDevices(); }
        int16_t RecordingDevices() override { return adm_->RecordingDevices(); }
        int32_t PlayoutDeviceName(uint16_t index,
            char name[webrtc::kAdmMaxDeviceNameSize],
            char guid[webrtc::kAdmMaxGuidSize]) override {
                return adm_->PlayoutDeviceName(index, name, guid);
        }
        int32_t RecordingDeviceName(uint16_t index,
            char name[webrtc::kAdmMaxDeviceNameSize],
            char guid[webrtc::kAdmMaxGuidSize]) override {
                return adm_->RecordingDeviceName(index, name, guid);
        }
        int32_t SetPlayoutDevice(uint16_t index) override { return adm_->SetPlayoutDevice(index); }
        int32_t SetPlayoutDevice(WindowsDeviceType device) override { return adm_->SetPlayoutDevice(device); }
        int32_t SetRecordingDevice(uint16_t index) override { return adm_->SetRecordingDevice(index); }
        int32_t SetRecordingDevice(WindowsDeviceType device) override { return adm_->SetRecordingDevice(device); }
        int32_t PlayoutIsAvailable(bool* available) override { return adm_->PlayoutIsAvailable(available);}
        int32_t InitPlayout() override { return adm_->InitPlayout(); }
        bool PlayoutIsInitialized() const override { return adm_->PlayoutIsInitialized(); }
        int32_t RecordingIsAvailable(bool* available) override { return adm_->RecordingIsAvailable(available); }
        int32_t InitRecording() override;
        bool RecordingIsInitialized() const override { return adm_->RecordingIsInitialized(); }
        int32_t StartPlayout() override { return adm_->StartPlayout(); }
        int32_t StopPlayout() override { return adm_->StopPlayout(); }
        bool Playing() const override { return adm_->Playing(); }
        int32_t StartRecording() override { return adm_->StartRecording(); }
        int32_t StopRecording() override { return adm_->StopRecording(); }
        bool Recording() const override { return adm_->Recording(); }
        int32_t InitSpeaker() override { return adm_->InitSpeaker(); }
        bool SpeakerIsInitialized() const override { return adm_->SpeakerIsInitialized(); }
        int32_t InitMicrophone() override { return adm_->InitMicrophone(); }
        bool MicrophoneIsInitialized() const override { return adm_->MicrophoneIsInitialized(); }
        int32_t SpeakerVolumeIsAvailable(bool* available) override { return adm_->SpeakerVolumeIsAvailable(available); }
        int32_t SetSpeakerVolume(uint32_t volume) override { return adm_->SetSpeakerVolume(volume); }
        int32_t SpeakerVolume(uint32_t* volume) const override { return adm_->SpeakerVolume(volume); }
        int32_t MaxSpeakerVolume(uint32_t* maxVolume) const override { return adm_->MaxSpeakerVolume(maxVolume); }
        int32_t MinSpeakerVolume(uint32_t* minVolume) const override { return adm_->MinSpeakerVolume(minVolume); }
        int32_t MicrophoneVolumeIsAvailable(bool* available) override { return adm_->MicrophoneVolumeIsAvailable(available); }
        int32_t SetMicrophoneVolume(uint32_t volume) override { return adm_->SetMicrophoneVolume(volume); }
        int32_t MicrophoneVolume(uint32_t* volume) const override { return adm_->MicrophoneVolume(volume); }
        int32_t MaxMicrophoneVolume(uint32_t* maxVolume) const override { return adm_->MaxMicrophoneVolume(maxVolume); }
        int32_t MinMicrophoneVolume(uint32_t* minVolume) const override { return adm_->MinMicrophoneVolume(minVolume); }
        int32_t SpeakerMuteIsAvailable(bool* available) override { return adm_->SpeakerMuteIsAvailable(available); }
        int32_t SetSpeakerMute(bool enable) override { return adm_->SetSpeakerMute(enable); }
        int32_t SpeakerMute(bool* enabled) const override { return adm_->SpeakerMute(enabled); }
        int32_t MicrophoneMuteIsAvailable(bool* available) override { return adm_->MicrophoneMuteIsAvailable(available); }
        int32_t SetMicrophoneMute(bool enable) override { return adm_->SetMicrophoneMute(enable); }
        int32_t MicrophoneMute(bool* enabled) const override { return adm_->MicrophoneMute(enabled); }
        int32_t StereoPlayoutIsAvailable(bool* available) const override { return adm_->StereoPlayoutIsAvailable(available); }
        int32_t SetStereoPlayout(bool enable) override { return adm_->SetStereoPlayout(enable); }
        int32_t StereoPlayout(bool* enabled) const override { return adm_->StereoPlayout(enabled); }
        int32_t StereoRecordingIsAvailable(bool* available) const override { return adm_->StereoRecordingIsAvailable(available); }
        int32_t SetStereoRecording(bool enable) override { return adm_->SetStereoRecording(enable); }
        int32_t StereoRecording(bool* enabled) const override { return adm_->StereoRecording(enabled); }
        int32_t PlayoutDelay(uint16_t* delayMS) const override { return adm_->PlayoutDelay(delayMS); }
        bool BuiltInAECIsAvailable() const override { return adm_->BuiltInAECIsAvailable(); }
        bool BuiltInAGCIsAvailable() const override { return adm_->BuiltInAGCIsAvailable(); }
        bool BuiltInNSIsAvailable() const override { return adm_->BuiltInNSIsAvailable(); }
        int32_t EnableBuiltInAEC(bool enable) override { return adm_->EnableBuiltInAEC(enable); }
        int32_t EnableBuiltInAGC(bool enable) override { return adm_->EnableBuiltInAGC(enable); }
        int32_t EnableBuiltInNS(bool enable) override { return adm_->EnableBuiltInNS(enable); }
#if defined(WEBRTC_IOS)
        virtual int GetPlayoutAudioParameters(webrtc::AudioParameters* params) const override { return adm_->GetPlayoutAudioParameters(params); }
        virtual int GetRecordAudioParameters(webrtc::AudioParameters* params) const override { return adm_->GetRecordAudioParameters(params); }
#endif

        void SetSelectedMicIndex(int index) { mic_index_ = index; }
        void SetMicVolumeToInitialize(float volume) { mic_volume_ = volume; }

    private:
        rtc::scoped_refptr<AudioDeviceModule> adm_;
        int mic_index_ = 0;
        float mic_volume_ = 1.0f;
    };

} // end namespace webrtc
} // end namespace unity
