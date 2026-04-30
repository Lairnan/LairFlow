using TestMorph.Entities;

var e = new TestEntity { Test = "hi" };
var dto = e.ToDto();

Console.WriteLine(dto.Test);