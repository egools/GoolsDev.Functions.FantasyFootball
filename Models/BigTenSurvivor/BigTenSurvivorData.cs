using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor
{
    public class BigTenSurvivorData
    {
        public ICollection<SurvivorPicker> Pickers { get; set; }

        public ICollection<SurvivorWeek> Schedule { get; set; }

        public ICollection<SurvivorSelection> UnmappedSelections { get; set; } = new List<SurvivorSelection>();

        public SurvivorPicker this[string name]
        {
            get => Pickers.FirstOrDefault(p => p.HasAlias(name));
        }

        public SurvivorWeek this[int weekNum]
        {
            get => Schedule.FirstOrDefault(w => w.WeekNum == weekNum);
        }
    }
}