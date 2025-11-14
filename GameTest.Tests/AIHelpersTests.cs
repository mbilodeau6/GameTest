using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Functions;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace GameTest.Tests;

public class AIHelpersTests
{
    [Fact]
    public void GetProbabilityForDiceRoll_TooLow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AIHelpers.GetProbabilityForDiceRoll(1));
    }

    [Fact]
    public void GetProbabilityForDiceRoll_TooHigh()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AIHelpers.GetProbabilityForDiceRoll(13));
    }

    [Fact]
    public void GetProbabilityForDiceRoll_Roll6()
    {
        var probability = AIHelpers.GetProbabilityForDiceRoll(8);

        Assert.Equal(5.0 / 36.0, probability);
    }
}



