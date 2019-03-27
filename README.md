# Waya

Сжатие изображений вейвлет-кодированием. 

Процесс сжатия: RGB -> Integer YUV -> Поканальный Wavelet (Y,U,V) -> Gzip (возможно заменю на Huffman)

Пока что Waya проигрывает популярным форматам, нужно оптимизировать.

Сравнение форматов:

![Jpeg](/images/lena_jpeg.jpg)

![PNG](/images/lena_png.jpg)

![Waya](/images/lena_waya.jpg)
