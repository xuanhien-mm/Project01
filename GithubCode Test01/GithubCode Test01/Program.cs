using System;

namespace GithubCode_Test01
{
    internal class Program
    {
        //Viết hàm tạo ma trận m, n với các phần tử là số nguyên sinh ngẫu nhiên
        static int[,] CreateRandomMatrix(int m, int n, int minValue, int maxValue)
        {
            Random rand = new Random();
            int[,] matrix = new int[m, n];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = rand.Next(minValue, maxValue + 1);
                }
            }
            return matrix;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Giải phương trình bậc 2 dạng: a x^2 + b x = 0");

            int a;
            int b;

            while (true)
            {
                Console.Write("Nhập số nguyên a: ");
                if (int.TryParse(Console.ReadLine(), out a))
                    break;
                Console.WriteLine("Giá trị không hợp lệ. Vui lòng nhập một số nguyên.");
            }

            while (true)
            {
                Console.Write("Nhập số nguyên b: ");
                if (int.TryParse(Console.ReadLine(), out b))
                    break;
                Console.WriteLine("Giá trị không hợp lệ. Vui lòng nhập một số nguyên.");
            }

            // Phương trình: a x^2 + b x = 0
            // Giải: x(ax + b) = 0 => nghiệm x = 0 và x = -b/a (nếu a != 0)

            if (a != 0)
            {
                double x1 = 0;
                double x2 = - (double)b / a;
                Console.WriteLine($"Phương trình: {a}x^2 + {b}x = 0");
                Console.WriteLine("Nghiệm:");
                Console.WriteLine($"x1 = {x1}");
                Console.WriteLine($"x2 = {x2}");
            }
            else
            {
                if (b != 0)
                {
                    // bx = 0 => x = 0
                    Console.WriteLine($"Phương trình: {b}x = 0");
                    Console.WriteLine("Nghiệm duy nhất: x = 0");
                }
                else
                {
                    // 0 = 0
                    Console.WriteLine("Phương trình 0 = 0: vô số nghiệm.");
                }
            }

            Console.WriteLine("Nhấn phím bất kỳ để kết thúc...");
            Console.ReadKey();
        }
    }
}
