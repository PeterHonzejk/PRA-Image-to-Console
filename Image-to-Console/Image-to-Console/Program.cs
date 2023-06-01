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
                Console.WriteLine("\nZadejte cestu k souboru s obrázkem ve formátu .bmp:");
                string filePath = Console.ReadLine();

                // Opakujte žádost o cestu k souboru, dokud není zadán platný vstup
                while (!File.Exists(filePath) || Path.GetExtension(filePath)?.ToLower() != ".bmp")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nNeplatný vstup nebo nepodporovaný formát obrázku. Podporován je pouze formát .bmp. Prosím, zkuste to znovu:");
                    Console.ResetColor();
                    filePath = Console.ReadLine();
                }

                // Načtěte soubor jako binární pole
                byte[] fileBytes = File.ReadAllBytes(filePath);

                // Získání informací o obrázku z hlavičky souboru
                int imageOffset = BitConverter.ToInt32(fileBytes, 0x0A);
                int imageWidth = BitConverter.ToInt32(fileBytes, 0x12);
                int imageHeight = BitConverter.ToInt32(fileBytes, 0x16);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nObrázek úspěšně načten!");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nInformace o obrázku:");
                Console.ResetColor();
                Console.WriteLine("Šířka: " + imageWidth);
                Console.WriteLine("Výška: " + imageHeight);

                // Výpočet poměru změny velikosti na základě rozměrů konzole
                int consoleWidth = Console.WindowWidth;
                int consoleHeight = Console.WindowHeight - 1;
                double widthRatio = (double)imageWidth / consoleWidth;
                double heightRatio = (double)imageHeight / consoleHeight;
                double resizeRatio = Math.Max(widthRatio, heightRatio);

                // Výpočet rozměrů zmenšeného obrázku
                int resizedWidth = (int)(imageWidth / resizeRatio);
                int resizedHeight = (int)(imageHeight / resizeRatio);

                // Vytvoření bufferu pro pixely zmenšeného obrázku
                byte[] resizedImageBuffer = new byte[resizedWidth * resizedHeight];

                // Zmenšení obrázku vzorkováním a kopírováním pixelů
                for (int y = 0; y < resizedHeight; y++)
                {
                    for (int x = 0; x < resizedWidth; x++)
                    {
                        // Výpočet původní pozice pixelu v obrázku
                        int originalX = (int)(x * resizeRatio);
                        int originalY = (int)(y * resizeRatio);

                        // Získání hodnoty pixelu z původního obrázku
                        byte pixelValue = fileBytes[imageOffset + originalY * imageWidth + originalX];

                        // Uložení hodnoty pixelu do zmenšeného bufferu
                        resizedImageBuffer[y * resizedWidth + x] = pixelValue;
                    }
                }

                Console.WriteLine("Rozměry zmenšeného obrázku:");
                Console.WriteLine("Šířka: " + resizedWidth);
                Console.WriteLine("Výška: " + resizedHeight + "\n");

                // Zobrazení zmenšeného obrázku v konzoli
                RenderImage(resizedImageBuffer, resizedWidth, resizedHeight);

                Console.WriteLine("\nPřejete si převést další obrázek? (y/n)");
                string choice = Console.ReadLine();

                if (choice?.ToLower() != "y")
                {
                    exitProgram = true;
                }
            }
        }

        static void RenderImage(byte[] imageBuffer, int imageWidth, int imageHeight)
        {
            int consoleWidth = Console.WindowWidth;
            int consoleHeight = Console.WindowHeight - 1;

            // Výpočet úpravy poměru stran pro znaky
            double aspectRatio = (double)consoleWidth / consoleHeight;
            double charAspectRatio = 2.0; // Upravte tuto hodnotu podle použitého písma

            // Výpočet poměrů šířky a výšky pro zobrazení obrázku v konzoli
            double widthRatio = (double)consoleWidth / imageWidth;
            double heightRatio = (double)consoleHeight / imageHeight;

            // Vyberte poměr změny velikosti na základě úpravy poměru stran
            double resizeRatio = Math.Min(widthRatio / charAspectRatio, heightRatio);

            // Výpočet rozměrů zmenšeného obrázku
            int resizedWidth = (int)(imageWidth * resizeRatio);
            int resizedHeight = (int)(imageHeight * resizeRatio);

            // Projděte pixely a vykreslete je v konzoli
            for (int y = 0; y < resizedHeight; y++)
            {
                for (int x = 0; x < resizedWidth; x++)
                {
                    // Získání hodnoty pixelu z bufferu
                    byte pixelValue = imageBuffer[(int)(y / resizeRatio) * imageWidth + (int)(x / resizeRatio)];

                    // Nastavení barvy pixelu na základě hodnoty pixelu
                    char pixelColor = pixelValue == 0 ? '#' : ' ';

                    // Výstup barvy pixelu do konzole
                    Console.Write(pixelColor);
                }
                Console.WriteLine();
            }
        }
    }
}