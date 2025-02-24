using Ductus.FluentDocker.Model.Compose;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Impl;
using Microsoft.Extensions.Hosting;

namespace WakeCommerce.BDD.Tests.Hook
{
    public static class DockerComposeService
    {
        public static ICompositeService CompositeService { get; private set; }
        public static IHostService DockerHost { get; private set; }

        public static void Dispose()
        {
            CompositeService.Dispose();
            DockerHost.Dispose();
        }

        public static async Task RunDockerCompose(string docker = "docker-compose.yml")
        {
            // Certifique-se de que o Docker Host está em execução
            EnsureDockerHost();

            // Caminho para o arquivo docker-compose.yml
            var projectDirectory = GetSolutionDirectory();

            string dockerComposeFile = Path.Combine(projectDirectory, docker);

            // Configuração para o Docker Compose
            var config = new DockerComposeConfig
            {
                ComposeFilePath = new[] { dockerComposeFile },
                ForceRecreate = true, // Forçar a recriação dos contêineres
                RemoveOrphans = true, // Remover contêineres órfãos
                StopOnDispose = true  // Parar os contêineres ao descartar a fixture
            };

            // Criar o serviço Docker Compose
            CompositeService = new DockerComposeCompositeService(DockerHost, config);

            // Iniciar os contêineres
            CompositeService.Start();

            await WaitForContainerAsync("WakeCommerce-WebApi");
        }

        public static async Task WaitForContainerAsync(string containerName, int timeoutSeconds = 60)
        {
            var startTime = DateTime.UtcNow;

            while ((DateTime.UtcNow - startTime).TotalSeconds < timeoutSeconds)
            {
                var container = CompositeService.Containers.Where(x => x.Name == containerName).FirstOrDefault();

                if (container != null && container.State == ServiceRunningState.Running)
                {
                    Console.WriteLine($"✅ Container '{containerName}' está rodando!");
                    return;
                }

                Console.WriteLine($"⌛ Aguardando container '{containerName}' iniciar...");

                await Task.Delay(2000); // Espera 2 segundos antes de tentar novamente
            }

            throw new TimeoutException($"❌ Timeout ao aguardar container '{containerName}' iniciar.");
        }

        private static void EnsureDockerHost()
        {
            // Verificar se o Docker Host está em execução
            if (DockerHost?.State == ServiceRunningState.Running)
                return;

            // Descobrir os hosts Docker disponíveis
            var hosts = new Hosts().Discover();

            // Selecionar o Docker Host nativo ou o padrão, se disponível
            DockerHost = hosts.FirstOrDefault(x => x.IsNative) ?? hosts.FirstOrDefault(x => x.Name == "default");

            // Iniciar o Docker Host, se necessário
            if (DockerHost?.State != ServiceRunningState.Running)
                DockerHost?.Start();
        }

        public static string GetSolutionDirectory()
        {
            // Obtém o diretório de trabalho atual
            string currentDirectory = Directory.GetCurrentDirectory();

            // Navega pelos diretórios pai até encontrar um arquivo .sln
            DirectoryInfo directory = new DirectoryInfo(currentDirectory);
            while (directory != null)
            {
                FileInfo[] solutionFiles = directory.GetFiles("*.sln");
                if (solutionFiles.Length > 0)
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }

            // Se nenhum arquivo .sln for encontrado, retorna null ou uma mensagem de erro
            throw new InvalidOperationException("Diretório da solução não encontrado.");
        }
    }
}

