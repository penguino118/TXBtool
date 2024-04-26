# Now archived
[Superseeded by TXB Editor](https://github.com/penguino118/TXBeditor)

## TXBtool
Simple tool that unpacks and repacks .TXB files from GioGio's Bizarre Adventure (PS2)

### Usage
To unpack a file use the `-u` command and the program will extract every TIM2 image from inside the file.
```
TXBtool.exe -u example.txb
```
To repack the TIM2 files to the .TXB file, use `-p` and then the filename of the original file.
```
TXBtool.exe -p example.txb
```
By default, the program will adjust the CLUT sizes of 16 color images to the correct size.<br>
To keep the CLUT size unmodified add `-k` at the end of the command.
```
TXBtool.exe -u example.txb -k
TXBtool.exe -p example.txb -k
```
