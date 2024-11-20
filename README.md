# URL Shceme Lancher
URL Scheme lancher decode filename from URL scheme and launch it by configured apps.

## How to use
You can open file from your browser by as following.
```
launchurl:exec=ImageEditor;file=https:example.com/image/imag.png
```
"exec" is app name which configured on "config.json" 

"file" is file path. It should be URL format started with "http://" "files://"

If file is the URL, this program download it to temporary folder, and launch app with file name argument.


## How to configure

```
{
  "programs": [
    {
      "name": "ImageEditor",
      "path": "mspaint"
    },
    {
      "name": "TextEditor",
      "path": "notepad.exe"
    }
  ]
}

```

## Installatiion
Use .msi installer file on release page.


### execution file
Installer make URL scheme "launchurl" on your Windows.

"URL_scheme.exe" will be installed to
```
%Program Files(x86%\)\Irid\URL Scheme Launcher\
```

### configuration file
Configulation file "config.json" will be installed to
```
%%userprofile%%\AppData\Roaming\Irid\URLSchemeLancher\
```
### registry
URL scheme setting will be added to registry.


## chrome extention
For example, sample chrome extension on chrome_extension directory.

Chrome and Edge browser can try it. 

Installing from local, developper mode will be requred.

