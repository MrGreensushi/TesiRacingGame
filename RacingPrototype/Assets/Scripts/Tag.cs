using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Tag 
{
   public Tag(string name,int value)
    {
        Name = name;
        Value = value;
    }
    public Tag(string name)
    {
        Name = name;
        Value = -1;
    }

    public string Name { get; }
    public int Value { get; }
    public bool Equals(Tag p) => Name.Equals( p.Name) || Value == p.Value;
}

public class TagManager
{
    public static readonly List<Tag> tags = new List<Tag>()
    {
        new Tag("C_15_D_R",0),
        new Tag("C_15_R_D",1),
        new Tag("C_15_D_L",2),
        new Tag("C_15_L_D",3),
        new Tag("C_15_R_U",4),
        new Tag("C_15_L_U",5),
        new Tag("C_15_U_L",6),
        new Tag("C_15_U_R",7),
        new Tag("C_30_D_R",8),
        new Tag("C_30_R_D",9),
        new Tag("C_30_D_L",10),
        new Tag("C_30_L_D",11),
        new Tag("C_30_R_U",12),
        new Tag("C_30_L_U",13),
        new Tag("C_30_U_L",14),
        new Tag("C_30_U_R",15),
        new Tag("S_D_U",16),
        new Tag("S_U_D",17),
        new Tag("S_L_R",18),
        new Tag("S_R_L",19)
    };
}

