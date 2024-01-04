using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace FoundationFortune.API.Core;

public sealed class VectorsConverter : IYamlTypeConverter
{
    private static readonly Dictionary<Type, Func<List<float>, object>> VectorConstructors = new()
    {
        { typeof(Vector2), coords => new Vector2(coords[0], coords[1]) },
        { typeof(Vector3), coords => new Vector3(coords[0], coords[1], coords[2]) },
        { typeof(Vector4), coords => new Vector4(coords[0], coords[1], coords[2], coords[3]) }
    };

    public bool Accepts(Type type) => VectorConstructors.ContainsKey(type);

    public object ReadYaml(IParser parser, Type type)
    {
        if (!parser.TryConsume<MappingStart>(out _)) throw new InvalidDataException($"Cannot deserialize object of type {type.FullName}.");

        List<float> coordinates = new();

        while (!parser.TryConsume<MappingEnd>(out _))
        {
            if (!parser.TryConsume<Scalar>(out var key) || !parser.TryConsume<Scalar>(out var value) || !float.TryParse(value.Value, NumberStyles.Float, CultureInfo.GetCultureInfo("en-US"), out float coordinate)) 
                throw new InvalidDataException("Invalid float value.");
            coordinates.Add(coordinate);
        }

        if (VectorConstructors.TryGetValue(type, out var constructor) && coordinates.Count >= constructor.Method.GetParameters().Length) return constructor(coordinates);
        throw new InvalidDataException($"Invalid number of coordinates for {type.FullName}.");
    }

    public void WriteYaml(IEmitter emitter, object value, Type type)
    {
        Dictionary<string, float> coordinates = new();

        switch (value)
        {
            case Vector2 vector2:
                coordinates["x"] = vector2.x;
                coordinates["y"] = vector2.y;
                break;
            case Vector3 vector3:
                coordinates["x"] = vector3.x;
                coordinates["y"] = vector3.y;
                coordinates["z"] = vector3.z;
                break;
            case Vector4 vector4:
                coordinates["x"] = vector4.x;
                coordinates["y"] = vector4.y;
                coordinates["z"] = vector4.z;
                coordinates["w"] = vector4.w;
                break;
            default:
                throw new ArgumentException($"Unsupported vector type: {type.FullName}");
        }

        emitter.Emit(new MappingStart());

        foreach (var coordinate in coordinates)
        {
            emitter.Emit(new Scalar(coordinate.Key));
            emitter.Emit(new Scalar(coordinate.Value.ToString(CultureInfo.GetCultureInfo("en-US"))));
        }

        emitter.Emit(new MappingEnd());
    }
}