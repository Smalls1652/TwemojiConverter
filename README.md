# TwemojiConverter [![Build ConsoleApp workflow](https://github.com/Smalls1652/TwemojiConverter/actions/workflows/build-consoleapp.yml/badge.svg?branch=main&event=push)](https://github.com/Smalls1652/TwemojiConverter/actions/workflows/build-consoleapp.yml) [![Generate PNGs](https://github.com/Smalls1652/TwemojiConverter/actions/workflows/generate-pngs.yml/badge.svg?branch=main&event=schedule)](https://github.com/Smalls1652/TwemojiConverter/actions/workflows/generate-pngs.yml)

This repo is for a CLI tool to convert Twemoji (Specifically [Discord's fork](https://github.com/discord/twemoji)) `SVG` files to `PNG` files. While there are `PNG` assets included in the Twemoji repo, they are lower resolution (`72px`). This tool allows for converting the `SVG` files to any resolution. In addition, it can also get various details about each emoji. It is taking advantage of [**.NET's Native AOT deployment model**](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=net8plus), so it doesn't require the .NET runtime to be installed to run the tool.

A workflow is setup in this repo to convert the `SVG` files to `PNG` files (In `128px`, `256px`, and `512px` variants) on a weekly basis. You can get the artifacts from any of the jobs [here](https://github.com/Smalls1652/TwemojiConverter/actions/workflows/generate-pngs.yml).

## 🏗️ Building from source

### 🧰 Pre-requisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
    - You will also need to install the pre-requisites for your platform [located here](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=net7%2Cwindows#prerequisites).
        - For Linux based platforms, you primarily need to ensure that packages for `clang` and `zlib` (dev) packages are installed to your system.

### 🧱 Building

> **⚠️ Note:**
> 
> Before building, you need to know the ["runtime identifier"](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog#known-rids) for your platform. For simplicity, these docs will use `linux-x64`. Replace that value with what you use, if needed.
> 
> For example if:
> * You're building on a **x64 Linux-based system**, the identifier would be `linux-x64`.
> * You're building on an **Apple-silicon macOS system**, the identifier would be `osx-arm64`.

#### Command-line

1. Set your current directory to where you cloned the repo.
2. Run the following command:

```plain
dotnet publish ./src/ConsoleApp/ --configuration "Release" --output "./build/" --runtime "linux-x64" --self-contained
```

The compiled binary will be located in the `./build/` directory in the local repo.

#### Visual Studio Code

1. Open the command palette (`Shift+Ctrl+P` **(Windows/Linux)** / `Shift+Cmd+P` **(macOS)**).
2. Type in **Tasks: Run Task** and press `Enter`.
   * **Ensure that is the selected option before pressing `Enter`.**
3. Select **Publish: ConsoleApp**.
4. Select your platform's runtime identifier.

The compiled binary will be located in the `./build/` directory in the local repo.

## 🗂️ Dependencies used

- [`Magick.NET`](https://github.com/dlemstra/Magick.NET)
    - This uses [ImageMagick](https://imagemagick.org).
- [`System.CommandLine`](https://github.com/dotnet/command-line-api)
- [`discord/twemoji`](https://github.com/discord/twemoji)
    - Indirectly used.

## 🤝 License

The source code for this project is licensed with the [MIT License](LICENSE).
