# Waya

Сжатие изображений вейвлет-кодированием. 

Процесс сжатия: RGB -> Integer YUV -> Поканальный Wavelet (Y,U,V) -> Gzip (возможно заменю на Huffman)

Пока что Waya проигрывает популярным форматам, нужно оптимизировать.

Сравнение форматов:

![Jpeg](/images/Lena_jpeg.jpg)

![PNG](/images/Lena_png.jpg)

![Waya](/images/Lena_waya.jpg)
