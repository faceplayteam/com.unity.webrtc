#include "pch.h"
#include "AudioDeviceWrapper.h"
#include <cassert>
#include "modules/audio_device/include/audio_device.h"
#include "api/task_queue/default_task_queue_factory.h"


namespace unity
{
namespace webrtc
{

    AudioDeviceWrapper::AudioDeviceWrapper()
        : adm_(webrtc::AudioDeviceModule::Create(
            webrtc::AudioDeviceModule::kPlatformDefaultAudio,
            webrtc::CreateDefaultTaskQueueFactory().get())) {
        assert(adm_ != nullptr);
    }

    int32_t AudioDeviceWrapper::InitRecording(){
        adm_->SetRecordingDevice(mic_index_);
        int32_t init = adm_->InitRecording();
        if (init == 0) {
            uint32_t volume = 0;
            if (adm_->MaxMicrophoneVolume(&volume) == 0) {
                adm_->SetMicrophoneVolume(mic_volume_ * volume);
            }
        }
        return init;
    }

} // end namespace webrtc
} // end namespace unity
