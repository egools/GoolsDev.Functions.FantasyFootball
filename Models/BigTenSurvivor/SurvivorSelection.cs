using System;

namespace GoolsDev.Functions.FantasyFootball.Models.BigTenSurvivor
{
    public class SurvivorSelection
    {
        public DateTime PickDateTime { get; set; }
        public string Name { get; set; }
        public int Week { get; set; }
        public string Team { get; set; }
        public bool? Correct { get; set; }
        public string SelectionStatus { get; set; }
    }
}