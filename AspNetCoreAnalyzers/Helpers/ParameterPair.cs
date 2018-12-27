namespace AspNetCoreAnalyzers
{
    using System;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;

    [DebuggerDisplay("{this.Route?.Name ?? this.Symbol.Name}")]
    public struct ParameterPair : IEquatable<ParameterPair>
    {
        public ParameterPair(TemplateParameter? route, IParameterSymbol symbol)
        {
            this.Route = route;
            this.Symbol = symbol;
        }

        public TemplateParameter? Route { get; }

        public IParameterSymbol Symbol { get; }

        public static bool operator ==(ParameterPair left, ParameterPair right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ParameterPair left, ParameterPair right)
        {
            return !left.Equals(right);
        }

        public bool Equals(ParameterPair other)
        {
            return this.Route.Equals(other.Route) &&
                   Equals(this.Symbol, other.Symbol);
        }

        public override bool Equals(object obj)
        {
            return obj is ParameterPair other &&
                   this.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Route.GetHashCode() * 397) ^ (this.Symbol != null ? this.Symbol.GetHashCode() : 0);
            }
        }
    }
}
