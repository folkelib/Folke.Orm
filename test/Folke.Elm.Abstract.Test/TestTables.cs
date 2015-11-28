﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Folke.Elm.Abstract.Test
{
    public class TestPoco : IFolkeTable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Boolean { get; set; }
    }

    public class TestManyPoco : IFolkeTable
    {
        public int Id { get; set; }
        public string Toto { get; set; }
        public TestPoco Poco { get; set; }
    }

    public class TestNotATable
    {
        public TestPoco Poco { get; set; }
        public TestManyPoco Many { get; set; }
    }

    public class TestMultiPoco : IFolkeTable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TestPoco One { get; set; }
        public TestPoco Two { get; set; }
        public TestManyPoco Three { get; set; }
    }

    public class TestCollectionMember : IFolkeTable
    {
        public int Id { get; set; }
        public TestCollection Collection { get; set; }
        public string Name { get; set; }
    }

    public class TestCollection : IFolkeTable
    {
        public int Id { get; set; }
        public IReadOnlyList<TestCollectionMember> Members { get; set; }
        public string Name { get; set; }
    }

    public class TestOtherPoco : IFolkeTable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Decimal { get; set; }
    }

    public class TestLinkTable : IFolkeTable
    {
        public int Id { get; set; }
        public TestPoco Group { get; set; }
        public TestOtherPoco User { get; set; }
    }

    [Table("TableWithGuid")]
    public class TableWithGuid
    {
        [Key]
        public Guid Id { get; set; }
        public string Text { get; set; }
    }

    public class ParentTableWithGuid
    {
        public Guid Key { get; set; }
        public string Text { get; set; }
        public TableWithGuid Reference { get; set; }
    }
}