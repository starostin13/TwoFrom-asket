namespace UnitRosterGenerator
{
    //// Категория юнита: неопытный, регулярный или ветеран
    //enum ExperienceLevel
    //{
    //    Inexperienced,
    //    Regular,
    //    Veteran
    //}


    // Класс, представляющий уровень опыта юнита
    class ExperienceLevelData
    {
        public string Level { get; set; }
        public int BaseCost { get; set; }
        public int AdditionalModelCost { get; set; }
        public Upgrade? Upgrades { get; set; } // Можно также использовать List<Upgrade>
    }
}
