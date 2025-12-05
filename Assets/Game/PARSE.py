import os

# Названия и расширения файлов
output_filename = 'result.txt'
file_extensions = ('.cs')

# Открываем (или создаем) итоговый файл для записи
with open(output_filename, 'w', encoding='utf-8') as outfile:
    # Рекурсивно обходим все папки и файлы, начиная с текущей директории
    for root, dirs, files in os.walk('.', topdown=True):
        # Исключаем папку 'node_modules' из дальнейшего обхода
        if 'node_modules' in dirs:
            dirs.remove('node_modules')
            
        for file in files:
            # Проверяем, что файл имеет нужное расширение,
            # не является 'package-lock.json'
            # и НЕ является тестовым файлом (не содержит '.test.js')
            if file.endswith(file_extensions) and file != 'package-lock.json' and '.test.js' not in file:
                # Формируем полный путь к файлу
                file_path = os.path.join(root, file)
                
                # Записываем путь к файлу в итоговый файл
                outfile.write(f'--- Путь к файлу: {file_path} ---\n\n')
                
                try:
                    # Открываем найденный файл для чтения
                    with open(file_path, 'r', encoding='utf-8', errors='ignore') as infile:
                        # Считываем содержимое и записываем в итоговый файл
                        content = infile.read()
                        outfile.write(content)
                        outfile.write('\n\n') # Добавляем пустые строки для разделения файлов
                except Exception as e:
                    # Если произошла ошибка при чтении файла, записываем сообщение об ошибке
                    outfile.write(f'*** Не удалось прочитать файл: {file_path}. Ошибка: {e} ***\n\n')

print(f'Все файлы были успешно собраны в {output_filename}')