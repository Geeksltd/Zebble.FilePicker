[logo]: https://raw.githubusercontent.com/Geeksltd/Zebble.FilePicker/master/icon.png "Zebble.FilePicker"


## Zebble.FilePicker

![logo]

A Zebble plugin to pick a media from library of device.


[![NuGet](https://img.shields.io/nuget/v/Zebble.FilePicker.svg?label=NuGet)](https://www.nuget.org/packages/Zebble.FilePicker/)

> It allows the user to upload a file from device media resource or turn on the camera to take a video or photo. It raises a popup that contains buttons for each valid resource to choose by the user.

<br>


### Setup
* Add the nuget package [https://www.nuget.org/packages/Zebble.Media/](https://www.nuget.org/packages/Zebble.Media/) in your project.
* Add the nuget package [https://www.nuget.org/packages/Zebble.FilePicker/](https://www.nuget.org/packages/Zebble.FilePicker/) in your project.
<br>


### Api Usage

To add file picker to the page you can use markup and c# code like below:
```xml
<FilePicker Id="MyFilePicker"></FilePicker>
```
```csharp
this.Add(new FilePicker { Id = "MyFilePicker" });
```
Also, you can set the valid resources by using AllowOnly() method. by default, add resources are allowed.
```csharp
MyFilePicker.Set(x => x.AllowOnly(MediaSource.PickPhoto, MediaSource.TakePhoto));
```

### Properties
| Property     | Type         | Android | iOS | Windows |
| :----------- | :----------- | :------ | :-- | :------ |
| AllowedSources           | MediaSource[]          | x       | x   | x       |
| VideoQuality   | VideoQuality  | x | x | x |
| File    | File Info | x | x | x |


### Methods
| Method       | Return Type  | Parameters                          | Android | iOS | Windows |
| :----------- | :----------- | :-----------                        | :------ | :-- | :------ |
| PickPhoto         | Task| -| x       | x   | x       |
| TakePhoto         | Task| -| x       | x   | x       |
| PickVideo         | Task| -| x       | x   | x       |
| TakeVideo         | Task| -| x       | x   | x       |
