using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SPCAMLQueryBuilder
{
    public enum QueryType
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        BeginsWith,
        EndsWith,
        Contains,
        DateRangesOverlap
    }

    public enum FSObjType
    {
        Document = 0,
        Folder = 1
    }

    public enum FieldType
    {
        Unknown,
        Text,
        Note,
        User,
        UserMulti,
        Boolean,
        Counter,
        Computed,
        Lookup,
        Integer,
        File,
        Date
    }
}
