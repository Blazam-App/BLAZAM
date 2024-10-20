﻿namespace BLAZAM.ActiveDirectory.Adapters
{
    public class ComputerMemory
    {
        public double Total { get; internal set; }
        public double Free { get; internal set; }
        public double PercentUsed => ((Total - Free) / Total) * 100;
    }
}