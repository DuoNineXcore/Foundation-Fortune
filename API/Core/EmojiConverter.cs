using System;
using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace FoundationFortune.API.Core;

public sealed class EmojiConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(string);

    public object ReadYaml(IParser parser, Type type)
    {
        var scalar = parser.Consume<Scalar>();
        return ConvertUnicodeEscapeSequenceToUPlusFormat(scalar.Value);
    }

    public void WriteYaml(IEmitter emitter, object value, Type type)
    {
        if (value == null)
        {
            emitter.Emit(new Scalar(null, null, "", ScalarStyle.Plain, true, false));
            return;
        }

        emitter.Emit(new Scalar(null, null, value.ToString(), ScalarStyle.Any, true, false));
    }

    /// <summary>
    /// specific ass method name
    /// </summary>
    private static string ConvertUnicodeEscapeSequenceToUPlusFormat(string input) => Regex.Replace(input, @"\\[Uu]([0-9A-Fa-f]{4,8})", m => "U+" + m.Groups[1].Value);
}