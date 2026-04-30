using LairMorph.Abstractions.Attributes;
using LairMorph.Abstractions.Interfaces;
using TestMorph.Entities;

namespace TestMorph.Dtos;

[Dto<TestEntity>]
public class TestEntityDto : IAutoMorphEntityDto<TestEntity>
{
    public string Test { get; set; }
}