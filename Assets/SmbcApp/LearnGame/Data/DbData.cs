using System;
using MasterMemory;
using MessagePack;

namespace SmbcApp.LearnGame.Data
{
    [MemoryTable("StockData")]
    [MessagePackObject(true)]
    public record StockData
    {
        [PrimaryKey] public int Id { get; init; }

        [SecondaryKey(0)]
        [NonUnique]
        [SecondaryKey(2)]
        [NonUnique]
        public DateTime Date { get; init; }

        [SecondaryKey(1)]
        [NonUnique]
        [SecondaryKey(2, 1)]
        [NonUnique]
        public int OrganizationId { get; init; }

        public int StockPrice { get; init; }
        public string OrganizationName { get; init; }
    }

    [MemoryTable("Organization")]
    [MessagePackObject(true)]
    public record OrganizationData
    {
        [PrimaryKey] public int Id { get; init; }
        public string Name { get; init; }
    }
}