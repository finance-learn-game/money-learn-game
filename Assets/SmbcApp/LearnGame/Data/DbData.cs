using System;
using MasterMemory;
using MessagePack;
using Sirenix.OdinInspector;

namespace SmbcApp.LearnGame.Data
{
    [MemoryTable("StockData")]
    [MessagePackObject(true)]
    public record StockData
    {
        [PrimaryKey] public int Id { get; init; }
        public int StockPrice { get; init; }
        public string OrganizationName { get; init; }

        // ReSharper disable all
        [SecondaryKey(0), NonUnique]
        [SecondaryKey(2), NonUnique]
        public DateTime Date { get; init; }

        [SecondaryKey(1), NonUnique]
        [SecondaryKey(2), NonUnique]
        public int OrganizationId { get; init; }

        // ReSharper restore all
    }

    [MemoryTable("Organization")]
    [MessagePackObject(true)]
    public record OrganizationData
    {
        [PrimaryKey] public int Id { get; init; }
        public string Name { get; init; }
    }

    [MemoryTable("News")]
    [MessagePackObject(true)]
    public record NewsData
    {
        [ShowInInspector] [PrimaryKey] public int Id { get; init; }
        [ShowInInspector] public string Detail { get; init; }
        [ShowInInspector] public string DirtyDate { get; init; }

        // ReSharper disable all
        [ShowInInspector]
        [SecondaryKey(0), NonUnique]
        [SecondaryKey(2), NonUnique]
        public int Year { get; init; }

        [ShowInInspector]
        [SecondaryKey(1), NonUnique]
        [SecondaryKey(2), NonUnique]
        public int Month { get; init; }
        // ReSharper restore all
    }
}