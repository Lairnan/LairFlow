using LairMorph.Abstractions.Attributes;
using LairMorph.Abstractions.Interfaces;
using TestMorph.Dtos;

namespace TestMorph.Entities;

[Entity<TestEntityDto>]
public class TestEntity : IAutoMorphEntity
{
    public string Test { get; set; }
}