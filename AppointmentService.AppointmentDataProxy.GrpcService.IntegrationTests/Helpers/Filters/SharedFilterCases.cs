using AppointmentService.AppointmentDataProxy.GrpcService.Protos;

namespace AppointmentService.AppointmentDataProxy.GrpcService.IntegrationTests.Helpers.Filters;

public static class SharedFilterCases
{
    public static readonly TheoryData<StringFilter, string, bool> StringCases = new()
    {
        { new StringFilter { Equals_ = "foo" }, "foo", true },
        { new StringFilter { Equals_ = "foo" }, "bar", false },
        { new StringFilter { Equals_ = "foo" }, "foobar", false },

        { new StringFilter { In = { "foo", "bar" } }, "foo", true },
        { new StringFilter { In = { "foo", "bar" } }, "bar", true },
        { new StringFilter { In = { "foo", "bar" } }, "baz", false },
        { new StringFilter { In = { "foo", "bar" } }, "foobar", false },

        { new StringFilter { NotIn = { "foo", "bar" } }, "foo", false },
        { new StringFilter { NotIn = { "foo", "bar" } }, "bar", false },
        { new StringFilter { NotIn = { "foo", "bar" } }, "something", true },
        { new StringFilter { NotIn = { "foo", "bar" } }, "other", true },

        { new StringFilter { Contains = "foo" }, "foo", true },
        { new StringFilter { Contains = "foo" }, "bar", false },
        { new StringFilter { Contains = "foo" }, "prefoosuf", true },
        { new StringFilter { Contains = "foo" }, "fo", false },

        { new StringFilter { StartsWith = "foo" }, "foo", true },
        { new StringFilter { StartsWith = "foo" }, "foobar", true },
        { new StringFilter { StartsWith = "foo" }, "barfoo", false },
        { new StringFilter { StartsWith = "foo" }, "fo", false },
        { new StringFilter { StartsWith = "foo" }, "_foo", false },
        { new StringFilter { StartsWith = "foo" }, " foo", false },

        { new StringFilter { EndsWith = "bar" }, "bar", true },
        { new StringFilter { EndsWith = "bar" }, "foobar", true },
        { new StringFilter { EndsWith = "bar" }, "foobar ", false },
        { new StringFilter { EndsWith = "bar" }, "foobar:", false },
        { new StringFilter { EndsWith = "bar" }, "barfoo", false },

        { new StringFilter { Equals_ = "foo", CaseInsensitive = true }, "foo", true },
        { new StringFilter { Equals_ = "foo", CaseInsensitive = true }, "FoO", true },
        { new StringFilter { Equals_ = "foo", CaseInsensitive = true }, "bar", false },
        { new StringFilter { Equals_ = "foo", CaseInsensitive = true }, "foobar", false },
    };

    public static readonly TheoryData<Int32Filter, int, bool> Int32Cases = new()
    {
        { new Int32Filter { Equals_ = 12 }, 12, true },
        { new Int32Filter { Equals_ = 12 }, 13, false },
        { new Int32Filter { Equals_ = 12 }, 11, false },
        { new Int32Filter { Equals_ = 12 }, -12, false },

        { new Int32Filter { In = { 1, 2, 3 } }, 1, true },
        { new Int32Filter { In = { 1, 2, 3 } }, 2, true },
        { new Int32Filter { In = { 1, 2, 3 } }, 3, true },
        { new Int32Filter { In = { 1, 2, 3 } }, 4, false },
        { new Int32Filter { In = { 1, 2, 3 } }, -1, false },

        { new Int32Filter { NotIn = { 1, 2 } }, 1, false },
        { new Int32Filter { NotIn = { 1, 2 } }, 2, false },
        { new Int32Filter { NotIn = { 1, 2 } }, 3, true },
        { new Int32Filter { NotIn = { 1, 2 } }, -1, true },

        { new Int32Filter { Min = 123 }, 123, true },
        { new Int32Filter { Min = 123 }, 124, true },
        { new Int32Filter { Min = 123 }, -124, false },
        { new Int32Filter { Min = 123 }, 122, false },

        { new Int32Filter { Max = 123 }, 124, false },
        { new Int32Filter { Max = 123 }, 123, true },
        { new Int32Filter { Max = 123 }, 122, true },

        { new Int32Filter { Min = 5, Max = 20 }, 4, false },
        { new Int32Filter { Min = 5, Max = 20 }, 5, true },
        { new Int32Filter { Min = 5, Max = 20 }, 10, true },
        { new Int32Filter { Min = 5, Max = 20 }, 20, true },
        { new Int32Filter { Min = 5, Max = 20 }, 21, false }
    };
}