<div align="center">

## 🎙️ VibeVoice: A Frontier Long Conversational Text-to-Speech Model
[![Project Page](https://img.shields.io/badge/Project-Page-blue?logo=microsoft)](https://microsoft.github.io/VibeVoice)
[![Hugging Face](https://img.shields.io/badge/HuggingFace-Collection-orange?logo=huggingface)](https://huggingface.co/collections/microsoft/vibevoice-68a2ef24a875c44be47b034f)
[![Technical Report](https://img.shields.io/badge/Technical-Report-red?logo=adobeacrobatreader)](report/TechnicalReport.pdf)
[![Live Playground](https://img.shields.io/badge/Live-Playground-green?logo=gradio)](https://aka.ms/VibeVoice-Demo)

</div>
<!-- <div align="center">
<img src="Figures/log.png" alt="VibeVoice Logo" width="200">
</div> -->

VibeVoice is a novel framework designed for generating **expressive**, **long-form**, **multi-speaker** conversational audio, such as podcasts, from text. It addresses significant challenges in traditional Text-to-Speech (TTS) systems, particularly in scalability, speaker consistency, and natural turn-taking.

A core innovation of VibeVoice is its use of continuous speech tokenizers (Acoustic and Semantic) operating at an ultra-low frame rate of 7.5 Hz. These tokenizers efficiently preserve audio fidelity while significantly boosting computational efficiency for processing long sequences. VibeVoice employs a [next-token diffusion](https://arxiv.org/abs/2412.08635) framework, leveraging a Large Language Model (LLM) to understand textual context and dialogue flow, and a diffusion head to generate high-fidelity acoustic details.

The model can synthesize speech up to **90 minutes** long with up to **4 distinct speakers**, surpassing the typical 1-2 speaker limits of many prior models. 


<p align="left">
  <img src="Figures/MOS-preference.png" alt="MOS Preference Results" height="260px">
  <img src="Figures/VibeVoice.jpg" alt="VibeVoice Overview" height="250px" style="margin-right: 10px;">
</p>


### 🎵 Demo Examples

**Cross-Lingual**
<div align="center">

https://github.com/user-attachments/assets/838d8ad9-a201-4dde-bb45-8cd3f59ce722

</div>

**Spontaneous Singing**
<div align="center">

https://github.com/user-attachments/assets/6f27a8a5-0c60-4f57-87f3-7dea2e11c730

</div>


**Long Conversation with 4 people**
<div align="center">

https://github.com/user-attachments/assets/a357c4b6-9768-495c-a576-1618f6275727

</div>

For more examples, see the [Project Page](https://microsoft.github.io/VibeVoice).

Try your own samples at [Demo](https://aka.ms/VibeVoice-Demo).


## Models
| Model | Context Length | Generation Length |  Weight |
|-------|----------------|----------|----------|
| VibeVoice-0.5B-Streaming | - | - | On the way |
| VibeVoice-1.5B | 64K | ~90 min | [HF link](https://huggingface.co/microsoft/VibeVoice-1.5B) |
| VibeVoice-7B| 32K | ~45 min | [HF link](https://huggingface.co/WestZhang/VibeVoice-Large-pt) |

## Installation
We recommend to use NVIDIA Deep Learning Container to manage the CUDA environment. 

1. Launch docker
```bash
# NVIDIA PyTorch Container 24.07 / 24.10 / 24.12 verified. 
# Later versions are also compatible.
sudo docker run --privileged --net=host --ipc=host --ulimit memlock=-1:-1 --ulimit stack=-1:-1 --gpus all --rm -it  nvcr.io/nvidia/pytorch:24.07-py3

## If flash attention is not included in your docker environment, you need to install it manually
## Refer to https://github.com/Dao-AILab/flash-attention for installation instructions
# pip install flash-attn --no-build-isolation
```

2. Install from github
```bash
git clone https://github.com/microsoft/VibeVoice.git
cd VibeVoice/

pip install -e .
```

## Usages

### Usage 1: Launch Gradio demo
```bash
apt update && apt install ffmpeg -y # for demo
python demo/gradio_demo.py --model_path microsoft/VibeVoice-1.5B --share
```

### Usage 2: Inference from files directly
```bash
# We provide some LLM generated example scripts under demo/text_examples/ for demo
# 1 speaker
python demo/inference_from_file.py --model_path microsoft/VibeVoice-1.5B --txt_path demo/text_examples/1p_abs.txt --speaker_names Alice

# or more speakers
python demo/inference_from_file.py --model_path microsoft/VibeVoice-1.5B --txt_path demo/text_examples/2p_music.txt --speaker_names Alice Yunfan
```

### Usage 3: Run the Windows desktop app

The repository now includes a native Windows desktop shell under `app/desktop/`.

Important behavior:

- you only launch the desktop app
- the desktop app manages the local runtime automatically
- you do **not** manually start a backend process before opening the app

#### Current status

This is the current developer workflow for the Windows app. It is already usable for local testing, but it is not yet packaged as a consumer installer.

#### Prerequisites

Before launching the desktop app, make sure:

1. Python dependencies are installed from this repository
2. `.NET 8 SDK` is installed on Windows
3. `ffmpeg` is available in `PATH`
4. your model checkpoints exist in `checkpoints/` or in another path you plan to load
5. your speaker prompt audio files exist in `demo/voices/`

#### Start the desktop app

From the repository root on Windows PowerShell:

```powershell
dotnet run --project app/desktop/VibeVoice.Desktop/VibeVoice.Desktop.csproj
```

You should see the native desktop window open directly.

The app will:

- check local runtime health
- auto-start the managed runtime if it is not already available
- show runtime and model readiness in the UI

## Windows Desktop App Workflow

The desktop shell follows this flow:

1. `Load Model`
2. `Generation Workspace`
3. `Job Progress`
4. `Result / Export`
5. `Settings`

### 1. Load Model

Use this section first.

What it does:

- shows runtime health
- shows device and ffmpeg readiness
- lets you point the app at a checkpoint directory
- loads the model before generation starts

How to use it:

1. Launch the desktop app
2. Check the runtime status text
3. Confirm or edit `Checkpoint Path`
4. Click `Load Model`
5. Wait until the model status shows that loading completed

If the path is wrong, model loading will fail and the error will appear in the `Errors` panel.

### 2. Speaker Presets

This section discovers available voice prompts from `demo/voices/`.

What it does:

- lists the detected voice presets
- lets you assign a prompt file to each speaker slot

How to use it:

1. Choose how many speakers you want to use later in the generation section
2. Assign matching voices in `Speaker 1` to `Speaker 4`
3. Only the first `N` selected voices are used, where `N` is the chosen speaker count

If you choose `2` speakers, only `Speaker 1` and `Speaker 2` are used.

### 3. Generation Workspace

This is where you configure the actual generation job.

Fields:

- `Speaker Count`: how many speakers are active, from `1` to `4`
- `CFG Scale`: guidance scale exposed in V1
- `Script`: the multi-speaker text content

How to format the script:

- recommended format:

```text
Speaker 0: Welcome to the show.
Speaker 1: Thanks, let's get started.
```

- if a line does not start with `Speaker X:`, the runtime will auto-assign speaker tags in order

How to use it:

1. Set `Speaker Count`
2. Adjust `CFG Scale` if needed
3. Paste or type the script
4. Click `Generate Job`

You can click `Stop Job` while generation is running.

### 4. Job Progress

This section makes the running job visible.

What it shows:

- current job state
- progress bar
- progress message
- elapsed time

The desktop app now consumes typed runtime events for progress and artifact readiness.

That means progress is driven by runtime job events instead of only periodic polling.

### 5. Result / Export

When generation completes successfully, this section becomes the main place to use the output.

What it shows:

- result summary
- artifact path
- playback controls
- export action

Buttons:

- `Play Result`: plays the generated artifact from its saved path
- `Stop Playback`: stops local playback
- `Export Copy`: copies the generated artifact into the configured output directory

If generation fails or is cancelled, the result summary will show the failure state instead of a successful artifact message.

### 6. Settings

This section stores desktop-local defaults.

Current V1 settings behavior:

- output directory can be edited
- runtime diagnostics are visible
- current defaults can be saved

When you click `Save Current Defaults`, the app persists:

- model path
- output directory
- speaker count
- CFG scale
- last script

## Files and Output Paths

Current runtime behavior:

- generated artifacts are written under `outputs/jobs/`
- exported copies are written to the output directory shown in `Settings`

If you do not change the output directory, the desktop app uses its saved local default.

## Typical End-to-End Example

1. Run:

```powershell
dotnet run --project app/desktop/VibeVoice.Desktop/VibeVoice.Desktop.csproj
```

2. In `Load Model`, set the checkpoint path and click `Load Model`
3. In `Speaker Presets`, select prompt audio for the speakers you plan to use
4. In `Generation Workspace`, set speaker count, review CFG scale, and enter script
5. Click `Generate Job`
6. Watch `Job Progress` update until completion
7. In `Result / Export`, click `Play Result` to audition the output
8. Click `Export Copy` if you want a copy in your configured output folder
9. If needed, update the output folder in `Settings` and click `Save Current Defaults`

## Troubleshooting

### The desktop app opens but model load fails

Check:

- the checkpoint path exists
- the checkpoint directory is compatible with the current runtime code
- Python dependencies finished installing correctly

### The app cannot find voice presets

Check:

- prompt audio files exist in `demo/voices/`
- the files use supported audio extensions such as `.wav`, `.mp3`, `.flac`, `.ogg`, `.m4a`, or `.aac`

### Playback does nothing

Check:

- the job actually completed
- the artifact path is not empty
- the generated file exists on disk

### Export fails

Check:

- the artifact file still exists
- the output directory is writable
- the destination is not blocked by permissions or antivirus

### Runtime does not become ready

Check:

- `python` is available in your environment
- local dependencies were installed with `pip install -e .`
- `ffmpeg` is available in `PATH`

## Current Limitations of the Windows App

- this repo currently ships a developer-run desktop workflow, not a packaged installer
- end-to-end QA with a real large model still needs more validation passes
- waveform editing, timeline editing, and cloud inference are not part of this Windows V1 flow

## FAQ
#### Q1: Is this a pretrained model?
**A:** Yes, it's a pretrained model without any post-training or benchmark-specific optimizations. In a way, this makes VibeVoice very versatile and fun to use.

#### Q2: Randomly trigger Sounds / Music / BGM.
**A:** As you can see from our demo page, the background music or sounds are spontaneous. This means we can't directly control whether they are generated or not. The model is content-aware, and these sounds are triggered based on the input text and the chosen voice prompt.

Here are a few things we've noticed:
*   If the voice prompt you use contains background music, the generated speech is more likely to have it as well. (The 7B model is quite stable and effective at this—give it a try on the demo!)
*   If the voice prompt is clean (no BGM), but the input text includes introductory words or phrases like "Welcome to," "Hello," or "However," background music might still appear.
*   Spekaer voice related, using "Alice" results in random BGM than others.
*   In other scenarios, the 7B model is more stable and has a lower probability of generating unexpected background music.

In fact, we intentionally decided not to denoise our training data because we think it's an interesting feature for BGM to show up at just the right moment. You can think of it as a little easter egg we left for you.

#### Q3: Text normalization?
**A:** We don't perform any text normalization during training or inference. Our philosophy is that a large language model should be able to handle complex user inputs on its own. However, due to the nature of the training data, you might still run into some corner cases.

#### Q4: Singing Capability.
**A:** Our training data **doesn't contain any music data**. The ability to sing is an emergent capability of the model (which is why it might sound off-key, even on a famous song like 'See You Again'). (The 7B model is more likely to exhibit this than the 1.5B).

#### Q5: Some Chinese pronunciation errors.
**A:** The volume of Chinese data in our training set is significantly smaller than the English data. Additionally, certain special characters in Chinese may lead to pronunciation problems.

## Risks and limitations

Potential for Deepfakes and Disinformation: High-quality synthetic speech can be misused to create convincing fake audio content for impersonation, fraud, or spreading disinformation. Users must ensure transcripts are reliable, check content accuracy, and avoid using generated content in misleading ways. Users are expected to use the generated content and to deploy the models in a lawful manner, in full compliance with all applicable laws and regulations in the relevant jurisdictions. It is best practice to disclose the use of AI when sharing AI-generated content.

English and Chinese only: Transcripts in languages other than English or Chinese may result in unexpected audio outputs.

Non-Speech Audio: The model focuses solely on speech synthesis and does not handle background noise, music, or other sound effects.

Overlapping Speech: The current model does not explicitly model or generate overlapping speech segments in conversations.

We do not recommend using VibeVoice in commercial or real-world applications without further testing and development. This model is intended for research and development purposes only. Please use responsibly.
