using CsvHelper;
using CsvHelper.Configuration.Attributes;

namespace MagicNumbersSimulation.Models
{
    public record ResultModel
    {
        [Index(0)]
        public uint Id { get; set; }
        [Index(1)]
        public ushort ScenarioNumber { get; set; }
        [Index(2)]
        public string SelectedNumbers { get; set; } = null!;
        [Index(3)]
        public ushort RightGuesses { get; set; }
        [Index(4)]
        public double Prize { get; set; }
    }
}
