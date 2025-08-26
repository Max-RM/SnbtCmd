# SnbtCmd - NBT/SNBT Converter

Конвертер между форматами NBT и SNBT для Minecraft.

## Совместимость

Программа совместима с Windows 7 и выше благодаря использованию .NET Framework 4.7.2.

## Использование

### Базовый синтаксис

```
SnbtCmd.exe <mode> [file_path] <conversion_type> [options] [--output-dir <directory>]
```

### Параметры

- `mode`: `path` или `raw`
  - `path` - указать путь к файлу
  - `raw` - читать из стандартного ввода

- `file_path`: путь к файлу (только для mode `path`)

- `conversion_type`: `to-snbt` или `to-nbt`
  - `to-snbt` - конвертировать NBT в SNBT
  - `to-nbt` - конвертировать SNBT в NBT

- `options`:
  - Для `to-snbt`: `expanded` - красивый вывод с отступами
  - Для `to-nbt`: `gzip` - сжатие GZip

- `--output-dir <directory>`: указать директорию для сохранения вывода
  - `source` - сохранить в папку исходного файла
  - `exe` - сохранить в папку с исполняемым файлом
  - `current` - сохранить в текущую рабочую директорию

### Поведение по умолчанию (важно)
- Для `to-snbt` (NBT → SNBT) без `--output-dir`: запись в `current` (текущая директория терминала), файл `out.txt`.
- Для `to-nbt` (SNBT → NBT) без `--output-dir`: запись в `exe` (рядом с `SnbtCmd.exe`), файл `out.txt`.

### Примеры использования

1. SNBT → NBT, сохранить рядом с exe (поведение по умолчанию):
   ```
   SnbtCmd.exe path "C:\data\level.snbt" to-nbt
   ```

2. NBT → SNBT, сохранить в текущей директории терминала (поведение по умолчанию):
   ```
   SnbtCmd.exe path "C:\Minecraft\world\level.dat" to-snbt expanded
   ```

3. Явно сохранить в текущей директории терминала:
   ```
   SnbtCmd.exe path "C:\data\level.dat" to-snbt --output-dir current
   ```

4. Явно сохранить рядом с exe:
   ```
   SnbtCmd.exe path "C:\data\level.snbt" to-nbt --output-dir exe
   ```

5. Чтение из stdin:
   ```
   type level.snbt | SnbtCmd.exe raw to-nbt --output-dir exe
   ```

## Сборка

### Требования
- .NET Framework 4.7.2 или выше
- Visual Studio 2019/2022 или .NET SDK

### Очистка
```
clean.bat
```
Удаляет `bin/`, `obj/` и каталог `publish/` во всех проектах.

### Компиляция по архитектурам
- x86:
```
publish_x86.bat
```
- x64:
```
publish_x64.bat
```
- ARM/ARM64 (если нужны и включены):
```
publish_arm32.bat
publish_arm64.bat
```

### Сборка всех архитектур
```
publish_all-architectures.bat
```
Собирает x86/x64 (и ARM/ARM64 при наличии). Если обнаружен `tools/ILRepack.exe`, дополнительно соберет единый exe в `publish/<rid>/single/SnbtCmd-single.exe`.

### Структура выходных файлов
После сборки в `publish/<rid>/` находятся:
- `SnbtCmd.exe`
- `*.dll` зависимостей (`TryashtarUtils.Nbt.dll`, `fNbt.dll`, `TryashtarUtils.Utility.dll`)
- `SnbtCmd.exe.config`

Если используется ILRepack: `publish/<rid>/single/SnbtCmd-single.exe` — единый exe.

## Изменения
- Добавлена совместимость с Windows 7 (переход на .NET Framework 4.7.2)
- Добавлен параметр `--output-dir` и новые значения по умолчанию
- Добавлены скрипты `clean.bat` и `publish_all-architectures.bat`
- Поддержка опциональной сборки единого exe через `tools/ILRepack.exe`
