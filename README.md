# AutoCadAddin

AutoCAD図面に線を描画するアドイン

## 機能
- AutoCAD上で `CL` コマンドを実行すると中心線を作成します
- AutoCAD上で `CE` コマンドを実行すると中心線を作成します
- AutoCAD上で `CCE` コマンドを実行すると円の中心線を作成します
- AutoCAD上で `XL` コマンドを実行すると下書線を作成します
- AutoCAD上で `XH` コマンドを実行すると下書線(水平)を作成します
- AutoCAD上で `XV` コマンドを実行すると下書線(垂直)を作成します

## 動作環境
- AutoCAD 2024
- .NET Framework 4.8
- Visual Studio 2022

## 使い方
1. Visual Studioでビルドして `AutoCadAddin.dll` を作成
2. AutoCADで `NETLOAD` コマンドを実行し、生成した `AutoCadAddin.dll` を読み込む
3. コマンドラインで各コマンドを実行

## 注意
商用のプラグインは含めていません。
実務のアドインとは異なり、機密情報や実際の図面名・ブロック名は含めていません。
