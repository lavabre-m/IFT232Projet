﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetIft232.Buildings
{
    [Obsolete("On ne devrait plus utiliser ça mis a part dans les tests, c'est pour ça qu'il y a le même enum dans le projet de test")]
    public enum BuildingType
    {
        House,
        Farm,
        Mine,
        SawMill,
        Carry,
        Casern,
        School,
        Market,
        Hospital,
        Null,
    }
}
