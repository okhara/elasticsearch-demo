﻿using System;
using System.Collections.Generic;

namespace PrimitiveApi
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public IList<string> Categories { get; set; }
        
        public IList<string> Manufacturers { get; set; }
    }
}
