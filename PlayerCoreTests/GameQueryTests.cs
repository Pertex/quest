﻿using QuestViva.PlayerCore;

namespace PlayerCoreTests;

[TestClass]
public class GameQueryTests
{
    [TestMethod]
    public async Task TestValidASL()
    {
        var query = new GameQuery("test1.asl");
        var result = await query.Initialise();
        Assert.IsTrue(result);
        Assert.AreEqual("Test ASL Game", query.GameName);
        Assert.AreEqual(410, query.ASLVersion);
        Assert.AreEqual("ACAB148143981E8F7F9A95151F4CB9F3", query.GameId);
        Assert.AreEqual(null, query.Category);
        Assert.AreEqual(null, query.Description);
    }

    [TestMethod]
    public async Task TestInvalidASL()
    {
        var query = new GameQuery("test2.asl");
        var result = await query.Initialise();
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    public async Task TestValidQuest()
    {
        var query = new GameQuery("test1.quest");
        var result = await query.Initialise();
        Assert.IsTrue(result);
        Assert.AreEqual("Test ASLX Game", query.GameName);
        Assert.AreEqual(520, query.ASLVersion);
        Assert.AreEqual("33cb328d-bf80-42f7-a136-e916e7b45ed8", query.GameId);
        Assert.AreEqual("Test Category", query.Category);
        Assert.AreEqual("Test Description", query.Description);
    }
    
    [TestMethod]
    public async Task TestInvalidQuest()
    {
        var query = new GameQuery("test2.quest");
        var result = await query.Initialise();
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task TestQuestResources()
    {
        var query = new GameQuery("resources.quest");
        var result = await query.Initialise();
        Assert.IsTrue(result);
        var resources = query.GetResourceNames()?.ToList();
        Assert.IsNotNull(resources);
        Assert.AreEqual(3, resources.Count);
        Assert.IsTrue(resources.Contains("game.aslx"));
        Assert.IsTrue(resources.Contains("aw.jpg"));
        Assert.IsTrue(resources.Contains("ta.png"));
        
        var stream = query.GetResource("aw.jpg");
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();
        Assert.AreEqual(0xd8, bytes[1]);
    }
}