![SnbtCmd exe](https://user-images.githubusercontent.com/81485476/132201154-01ebb9e2-e7c4-48dd-8f1c-3dde083f6ed2.png)
# SnbtCmd
SNBT to NBT and NBT to SNBT converter.

This program was created by Tryashtar on August 30, 2021. written in C#, you can use it to convert NBT files to SNBT and back. this is very useful.

■usage examples■

Use:
First type mode 'path' or 'raw'
If using 'path', next type the file path
Next type 'to-snbt' or 'to-nbt'
If using 'to-snbt', add 'expanded' for pretty-print)
If using 'to-nbt', add 'gzip' for g-zip compression)

write the code to a file with the extension cmd or bat.
to run it, double-click on the file containing your code.

example:

1) SnbtCmd.exe path "file path" to-snbt > out.txt
"file path" specify the full path to the NBT file. or, if the file is located in the same folder with SnbtCmd.exe and your executable cmd file, you can enter in the" file path " just the name of the NBT file.
the to-snbt module converts an NBT file to snbt
out.txt will be an snbt file containing the snbt. (you can change its name)

2) SnbtCmd.exe path "file path" to-nbt" > out.nbt
