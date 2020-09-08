# Package Sample
<!-- Describe your package -->

[![NPM Package](https://img.shields.io/npm/v/com.microsoft.fizzyo)](https://www.npmjs.com/package/com.microsoft.fizzyo)
[![openupm](https://img.shields.io/npm/v/com.microsoft.fizzyo?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.microsoft.fizzyo/)
[![Licence](https://img.shields.io/npm/l/com.microsoft.fizzyo)](https://github.com/StansAssets/com.microsoft.fizzyo/blob/master/LICENSE)
[![Issues](https://img.shields.io/github/issues/StansAssets/com.microsoft.fizzyo)](https://github.com/StansAssets/com.microsoft.fizzyo/issues)

<!-- Add some useful links here -->

[API Reference](https://myapi) | [Forum](https://myforum) | [Wiki](https://github.com/StansAssets/com.microsoft.fizzyo/wiki)

### Install from NPM
* Navigate to the `Packages` directory of your project.
* Adjust the [project manifest file](https://docs.unity3d.com/Manual/upm-manifestPrj.html) `manifest.json` in a text editor.
* Ensure `https://registry.npmjs.org/` is part of `scopedRegistries`.
  * Ensure `com.stansassets` is part of `scopes`.
  * Add `com.microsoft.fizzyo` to the `dependencies`, stating the latest version.

A minimal example ends up looking like this. Please note that the version `X.Y.Z` stated here is to be replaced with [the latest released version](https://www.npmjs.com/package/com.stansassets.foundation) which is currently [![NPM Package](https://img.shields.io/npm/v/com.stansassets.foundation)](https://www.npmjs.com/package/com.stansassets.foundation).
  ```json
  {
    "scopedRegistries": [
      {
        "name": "npmjs",
        "url": "https://registry.npmjs.org/",
        "scopes": [
          "com.stansassets"
        ]
      }
    ],
    "dependencies": {
      "com.microsoft.fizzyo": "X.Y.Z",
      ...
    }
  }
  ```
* Switch back to the Unity software and wait for it to finish importing the added package.

### Install from OpenUPM
* Install openupm-cli `npm install -g openupm-cli` or `yarn global add openupm-cli`
* Enter your unity project folder `cd <YOUR_UNITY_PROJECT_FOLDER>`
* Install package `openupm add com.microsoft.fizzyo`

### Install from a Git URL
Yoy can also install this package via Git URL. To load a package from a Git URL:

* Open [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui.html) window.
* Click the add **+** button in the status bar.
* The options for adding packages appear.
* Select Add package from git URL from the add menu. A text box and an Add button appear.
* Enter the `https://github.com/StansAssets/com.microsoft.fizzyo.git` Git URL in the text box and click Add.
* You may also install a specific package version by using the URL with the specified version.
  * `https://github.com/StansAssets/com.microsoft.fizzyo#X.Y.X`
  * Please note that the version `X.Y.Z` stated here is to be replaced with the version you would like to get.
  * You can find all the available releases [here](https://github.com/StansAssets/com.microsoft.fizzyo/releases).
  * The latest available release version is [![Last Release](https://img.shields.io/github/v/release/stansassets/com.microsoft.fizzyo)](https://github.com/StansAssets/com.microsoft.fizzyo/releases/latest)

For more information about what protocols Unity supports, see [Git URLs](https://docs.unity3d.com/Manual/upm-git.html).

