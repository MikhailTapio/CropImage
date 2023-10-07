using SixLabors.ImageSharp.Formats.Png;

internal class Program
{
    static Dictionary<string, List<string>> outputDigest = new();
    private static void Main(string[] args)
    {
        string inputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "input");
        if (!Directory.Exists(inputFolder))
        {
            Directory.CreateDirectory(inputFolder);
        }

        string outputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        Console.WriteLine("请将所有待裁剪图片放入本软件目录下的input文件夹，按回车键继续。");
        Console.ReadLine();

        string[] imgFiles = Directory.GetFiles(inputFolder, "*.*", SearchOption.AllDirectories)
            .Where(file => file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".jpg"))
            .ToArray();

        foreach (string imgFile in imgFiles)
        {
            try
            {
                using var image = Image.Load(imgFile);
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(ProcessSize(image.Width), ProcessSize(image.Height)),
                    Mode = ResizeMode.Crop
                }));

                string outputPath = Path.ChangeExtension(Path.Combine(outputFolder, Path.GetFileName(imgFile)), "png");
                image.Save(outputPath, new PngEncoder());
                Console.WriteLine($"图像已保存到: {outputPath}");
                AddToDigest(GenResolutionStr(image.Width, image.Height), outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理图像时发生错误: {ex.Message}");
            }
        }

        string digestPath = Path.Combine(outputFolder, "digest.txt");
        GenDigest(digestPath);
        Console.WriteLine("处理完成，输出概要已打印至" + digestPath + "。按回车键退出。");
        Console.ReadLine();

    }

    private static int ProcessSize(int raw)
    {
        if (raw < 64) return raw;
        return raw & (~63);
    }

    private static string GenResolutionStr(int width, int height)
    {
        return width + "x" + height;
    }

    private static void AddToDigest(string resolution, string path)
    {
        if (!outputDigest.ContainsKey(resolution)) outputDigest.Add(resolution, new List<string> {});
        outputDigest.GetValueOrDefault(resolution, new List<string> {}).Add(path);
    }

    private static void GenDigest(string path)
    {
        List<string> info = new();
        foreach (string key in outputDigest.Keys)
        {
            List<string> files = outputDigest.GetValueOrDefault(key, new List<string> {});
            int count = files.Count;
            if (count == 0) continue;
            info.Add(key + ": " + count);
            foreach (string str in files) info.Add(str);
        }
        using StreamWriter writer = new(path);
        foreach (string line in info)
        {
            writer.WriteLine(line);
        }
    }
}