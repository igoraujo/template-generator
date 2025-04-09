using System.Text;
using Stubble.Core.Builders;
using Microsoft.Extensions.Configuration;

namespace TemplateGenerator;

static class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Lê dados do appsettings.json
        string templateRoot = config["TemplateRoot"];
        string outputRoot = config["OutputRoot"];
        string defaultProjectName = config["DefaultProjectName"];

        var data = config.GetSection("Data")
                         .GetChildren()
                         .ToDictionary(x => x.Key, x => x.Value);

        // Pergunta o nome do projeto e sobrescreve o valor
        Console.WriteLine("🎹 Digite o nome do projeto:");
        string projectName = Console.ReadLine();
        data["projectName"] = projectName; // lowercase
        data["ProjectName"] = projectName; // PascalCase, se usar

        var stubble = new StubbleBuilder().Build();

        // Encontra todos os arquivos .mustache na pasta de templates
        var templateFiles = Directory.GetFiles(templateRoot, "*.mustache", SearchOption.AllDirectories);

        foreach (var templateFile in templateFiles)
        {
            Console.WriteLine($"\n🧩 Gerando com template: {templateFile}");

            // Lê o conteúdo do template
            string templateContent = File.ReadAllText(templateFile);

            // Renderiza o conteúdo
            string renderedContent = await stubble.RenderAsync(templateContent, data);

            // Calcula caminho relativo (da raiz da pasta templates)
            string relativePath = Path.GetRelativePath(templateRoot, templateFile);

            // Remove extensão .mustache
            relativePath = relativePath.Replace(".mustache", "");

            // Renderiza o caminho (substituindo {{projectName}}, etc)
            string renderedPath = await stubble.RenderAsync(relativePath, data);

            // Caminho final de saída
            string outputPath = Path.Combine(outputRoot, renderedPath);

            // Cria diretórios se necessário
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            // Salva o arquivo
            File.WriteAllText(outputPath, renderedContent);

            Console.WriteLine($"✅ Gerado: {outputPath}");
        }

        Console.WriteLine($"\n🎉 Todos os arquivos foram gerados com sucesso!");
    }
}
