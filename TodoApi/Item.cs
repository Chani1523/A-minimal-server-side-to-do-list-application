﻿using System;
using System.Collections.Generic;

namespace TodoApi;


public partial class Item
{
    public int IdItems { get; set; }

    public string Name { get; set; } = null!;

    public bool IsComplete { get; set; }
}
