using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;

namespace WakeCommerce.BDD.Tests.Hook
{
    [Binding]
    public class DocHook
    {
        private static ExtentTest _feature; // nodo para a Feature
        private static ExtentTest _scenario; // nodo para o Scenario
        private static ExtentReports _extent; // objeto do ExtentReports que será criado
        private static ScenarioContext _scenarioContext;

        public DocHook(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeTestRun]
        public static void ConfigureReport()
        {
            // aqui informo o caminho do arquivo que será gerado criando um objeto ExtentHtmlReporter
            string reportPath = Path.Combine(GetSolutionDirectory(), "TestResults");
            string reportDirectory = Path.GetDirectoryName(reportPath);

            
            if (!Directory.Exists(reportDirectory))
            {
                Directory.CreateDirectory(reportDirectory);
            }

            var htmlReporter = new ExtentSparkReporter($"{reportDirectory}/ExtentReport.html");

            // instancio o objeto ExtentReports
            _extent = new ExtentReports();

            // aqui dou attach no ExtentHtmlReporter
            _extent.AttachReporter(htmlReporter);
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            _feature = _extent.CreateTest($"Cenário: {featureContext.FeatureInfo.Title}");
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _scenario = _feature.CreateNode(_scenarioContext.ScenarioInfo.Title);
        }

        [AfterStep]
        public void InsertReportingSteps()
        {
            var stepType = _scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
            var stepName = _scenarioContext.StepContext.StepInfo.Text;

            if (_scenarioContext.TestError == null)
            {
                _scenario.CreateNode(new GherkinKeyword(stepType), $"{TranslateStep(stepType)}: {stepName}");
            }
            else
            {
                _scenario.CreateNode(new GherkinKeyword(stepType), $"{TranslateStep(stepType)}: {stepName}")
                         .Fail(_scenarioContext.TestError.Message);
            }
        }

        private string TranslateStep(string step)
        {
            switch (step)
            {
                case "Given":
                    return "Dado";
                case "When":
                    return "Quando";
                case "Then":
                    return "Então";
                case "And":
                    return "E";
                case "But":
                    return "Mas";
                default:
                    return step;
            }
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _extent.Flush();
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