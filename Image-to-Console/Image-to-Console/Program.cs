using System;
using System.IO;

namespace Image_to_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            bool exitProgram = false;

            while (!exitProgram)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("\nZadejte cestu k obrázku ve formátu .bmp:");
                Console.ResetColor();

                string filePath = Console.ReadLine();

                // Ověření platnosti zadané cesty k souboru
                while (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath) || Path.GetExtension(filePath)?.ToLower() != ".bmp")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nNeplatný vstup nebo nepodporovaný formát obrázku. Zkuste to znovu:");
                    Console.ResetColor();
                    filePath = Console.ReadLine();
                }

                byte[] fileBytes = ReadImageFile(filePath);

                // Získání informací o obrázku z hlavičky souboru
                int imageOffset = BitConverter.ToInt32(fileBytes, FileHeaderOffsets.ImageOffset);
                int imageWidth = BitConverter.ToInt32(fileBytes, FileHeaderOffsets.ImageWidth);
                int imageHeight = BitConverter.ToInt32(fileBytes, FileHeaderOffsets.ImageHeight);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nObrázek úspěšně načten!");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nInformace o obrázku:");
                Console.ResetColor();
                Console.WriteLine("Šířka: " + imageWidth);
                Console.WriteLine("Výška: " + imageHeight);

                int consoleWidth = Console.WindowWidth;
                int consoleHeight = Console.WindowHeight - 1;

                // Výpočet poměru změny velikosti na základě rozměrů konzole
                double resizeRatio = CalculateResizeRatio(imageWidth, imageHeight, consoleWidth, consoleHeight);

                int resizedWidth = (int)(imageWidth / resizeRatio);
                int resizedHeight = (int)(imageHeight / resizeRatio);

                byte[] resizedImageBuffer = ResizeImage(fileBytes, imageOffset, imageWidth, imageHeight, resizedWidth, resizedHeight, resizeRatio);

                Console.WriteLine("Rozměry zmenšeného obrázku:");
                Console.WriteLine("Šířka: " + resizedWidth);
                Console.WriteLine("Výška: " + resizedHeight + "\n");

                RenderImage(resizedImageBuffer, resizedWidth, resizedHeight);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nPřejete si převést další obrázek? (y/n)");
                Console.ResetColor();
                string choice = Console.ReadLine();

                if (choice?.ToLower() != "y")
                {
                    exitProgram = true;
                }
            }
        }

        static byte[] ReadImageFile(string filePath)
        {
            byte[] fileBytes;

            try
            {
                fileBytes = File.ReadAllBytes(filePath);
            }
            catch (IOException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nChyba při čtení souboru: " + e.Message);
                Console.ResetColor();
                fileBytes = new byte[0];
            }

            return fileBytes;
        }

        static double CalculateResizeRatio(int imageWidth, int imageHeight, int consoleWidth, int consoleHeight)
        {
            double widthRatio = (double)imageWidth / consoleWidth;
            double heightRatio = (double)imageHeight / consoleHeight;

            return Math.Max(widthRatio, heightRatio);
        }

        static byte[] ResizeImage(byte[] fileBytes, int imageOffset, int imageWidth, int imageHeight, int resizedWidth, int resizedHeight, double resizeRatio)
        {
            byte[] resizedImageBuffer = new byte[resizedWidth * resizedHeight];

            for (int y = 0; y < resizedHeight; y++)
            {
                for (int x = 0; x < resizedWidth; x++)
                {
                    int originalX = (int)(x * resizeRatio);
                    int originalY = (int)(y * resizeRatio);

                    byte pixelValue = fileBytes[imageOffset + originalY * imageWidth + originalX];

                    resizedImageBuffer[y * resizedWidth + x] = pixelValue;
                }
            }

            return resizedImageBuffer;
        }

        static void RenderImage(byte[] imageBuffer, int imageWidth, int imageHeight)
        {
            int consoleWidth = Console.WindowWidth;
            int consoleHeight = Console.WindowHeight - 1;

            double aspectRatio = (double)consoleWidth / consoleHeight;
            double charAspectRatio = 2.0;

            double widthRatio = (double)consoleWidth / imageWidth;
            double heightRatio = (double)consoleHeight / imageHeight;

            double resizeRatio = Math.Min(widthRatio / charAspectRatio, heightRatio);

            int resizedWidth = (int)(imageWidth * resizeRatio);
            int resizedHeight = (int)(imageHeight * resizeRatio);

            for (int y = 0; y < resizedHeight; y++)
            {
                for (int x = 0; x < resizedWidth; x++)
                {
                    byte pixelValue = imageBuffer[(int)(y / resizeRatio) * imageWidth + (int)(x / resizeRatio)];

                    char pixelColor = pixelValue == 0 ? '#' : ' ';

                    Console.Write(pixelColor);
                }
                Console.WriteLine();
            }
        }

        // Konstanty pro ofsety hlavičky souboru
        static class FileHeaderOffsets
        {
            public const int ImageOffset = 0x0A;
            public const int ImageWidth = 0x12;
            public const int ImageHeight = 0x16;
        }
    }
}
