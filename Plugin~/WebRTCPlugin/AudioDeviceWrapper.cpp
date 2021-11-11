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

    int32 AudioDeviceWrapper::InitRecording(){
        adm_->SetRecordingDevice(mic_index_);
        return adm_->InitRecording();
    }

} // end namespace webrtc
} // end namespace unity
