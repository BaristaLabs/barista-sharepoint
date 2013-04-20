namespace Barista
{
  using System;

  [Flags]
  public enum PrincipalType
  {
    None = 0,
    User = 1,
    DistributionGroup = 2,
    SecurityGroup = 4,
    All = User | DistributionGroup | SecurityGroup | SecurityGroup
  }
}
