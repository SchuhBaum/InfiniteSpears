namespace InfiniteSpears;

public static class RainMeadowMod {
    //
    // initialization

    private static Hook? hook_OnlineEntity_OnReadState;

    internal static void OnEnable() {
        hook_OnlineEntity_OnReadState?.Dispose();
        hook_OnlineEntity_OnReadState = null;

        var read_state_method = typeof(OnlineEntity).GetMethod(
            nameof(OnlineEntity.ReadState),
            new[] { typeof(OnlineEntity.EntityState), typeof(OnlineResource) });
        if (read_state_method == null) {
            throw new MissingMethodException($"{mod_id}: Failed to find Rain Meadow's OnlineEntity.ReadState method.");
        }

        hook_OnlineEntity_OnReadState = new Hook(read_state_method, typeof(RainMeadowMod).GetMethod(
            nameof(OnlineEntity_OnReadState), BindingFlags.Static | BindingFlags.NonPublic)!);
    }

    //
    // RPCs

    [SoftRPCMethod]
    public static void
    ReceiveSettings(
        RPCEvent response,
        OnlineEntity.EntityId online_player_id,
        int max_spear_count)
    {
        if (online_player_id.FindEntity(quiet: true) is not OnlineCreature online_player) return;
        if (online_player.isMine) return;
        if (response.from != online_player.owner) return;

        if (online_player.apo is not AbstractCreature abstract_player) return;

        if (!_all_attached_fields.ContainsKey(abstract_player)) {
            _all_attached_fields.Add(abstract_player, new Attached_Fields(max_spear_count));
        } else {
            _all_attached_fields[abstract_player] = new Attached_Fields(max_spear_count);
        }

        SyncBackSpears(online_player);
    }

    [SoftRPCMethod]
    public static GenericResult
    RequestSettings(
        RPCEvent request,
        OnlineEntity.EntityId online_player_id)
    {
        if (online_player_id.FindEntity(quiet: true) is not OnlineCreature online_player || !online_player.isMine) {
            return new GenericResult.Fail(request);
        }

        var abstract_player = online_player.apo as AbstractCreature;
        if (abstract_player == null) {
            return new GenericResult.Fail(request);
        }

        var player = abstract_player.realizedCreature as Player;
        if (player == null) {
            return new GenericResult.Fail(request);
        }

        var attached_fields = player.Get_Attached_Fields();
        request.from.InvokeRPC(ReceiveSettings, online_player.id, attached_fields.max_spear_count);
        return new GenericResult.Ok(request);
    }

    //
    // public

    public static void
    InvokeRequestSettings(
        AbstractCreature abstract_player)
    {
        if (OnlineManager.lobby == null) return;

        var online_player = abstract_player.GetOnlineObject();
        if (online_player == null) return;

        online_player.owner.InvokeRPC(RequestSettings, online_player.id);
    }

    public static bool
    IsAbstractPhysicalObjectMine(
        AbstractPhysicalObject? abstract_object) {
        if (abstract_object == null) {
            return true;
        }

        var online_object = abstract_object.GetOnlineObject();
        if (online_object == null) {
            return true;
        }

        return online_object.isMine;
    }

    public static void
    LogBackSticks(
        string stage,
        AbstractCreature? abstract_player,
        AbstractPhysicalObject? target_spear = null)
    {
        if (abstract_player == null) return;

        try
        {
            var player = abstract_player.realizedCreature as Player;
            if (player == null) return;

            var attached_fields = player.Get_Attached_Fields();
            Debug.Log($"{mod_id}: {stage}: player {abstract_player.ID}, "
                + $"stuckObjects={abstract_player.stuckObjects.Count}, "
                + $"localBackList={attached_fields.abstract_on_back_sticks.Count}, "
                + $"target={target_spear?.ID}, targetOnline={target_spear?.GetOnlineObject()?.id}.");

            for (int index = 0; index < abstract_player.stuckObjects.Count; ++index) {
                var entry = abstract_player.stuckObjects[index];
                if (entry == null) {
                    Debug.Log($"{mod_id}: {stage}: stuckObjects[{index}] is actually null.");
                    continue;
                }

                if (entry is AbstractOnBackStick back_stick) {
                    Debug.Log($"{mod_id}: {stage}: stuckObjects[{index}] is AbstractOnBackStick: "
                        + $"A={back_stick.Player?.ID}, AOnline={back_stick.Player?.GetOnlineObject()?.id}, "
                        + $"B={back_stick.Spear?.ID}, BOnline={back_stick.Spear?.GetOnlineObject()?.id}, "
                        + $"BRealized={back_stick.Spear?.realizedObject != null}, "
                        + $"playerMatches={ReferenceEquals(back_stick.Player, abstract_player)}, "
                        + $"targetMatches={ReferenceEquals(back_stick.Spear, target_spear)}.");
                } else {
                    Debug.Log($"{mod_id}: {stage}: stuckObjects[{index}] is {entry.GetType().FullName}: "
                        + $"A={entry.A?.ID}, B={entry.B?.ID}.");
                }
            }
        }
        catch { }
    }

    public static void
    SyncBackSpears(
        OnlineCreature online_player)
    {
        if (online_player.isMine) return;
        if (online_player.apo is not AbstractCreature abstract_player) return;

        var player = abstract_player.realizedCreature as Player;
        if (player == null) return;

        var attached_fields = player.Get_Attached_Fields();
        if (attached_fields.is_blacklisted) return;
        if (attached_fields.has_infinite_spears) return;
        if (attached_fields.max_spear_count <= 0) return;

        var count_before = attached_fields.abstract_on_back_sticks.Count;

        var back_sticks = abstract_player.stuckObjects
            .OfType<AbstractOnBackStick>()
            .Where(stick => stick.Player == abstract_player && stick.Spear is AbstractSpear)
            .Take(attached_fields.max_spear_count)
            .ToList();
        attached_fields.abstract_on_back_sticks.Clear();
        attached_fields.abstract_on_back_sticks.AddRange(back_sticks);

        var count_after = attached_fields.abstract_on_back_sticks.Count;

        // if (count_before != count_after) {
        //     Debug.Log($"DEBUG: SyncBackSpears for {online_player.id}. {abstract_player.stuckObjects.Count} {back_sticks.Count} {count_before} {count_after} {attached_fields.max_spear_count}");
        //     LogBackSticks("DEBUG: SyncBackSpears", online_player.apo as AbstractCreature);
        // }
    }

    //
    // private

    private static void
    OnlineEntity_OnReadState(
        Action<OnlineEntity, OnlineEntity.EntityState, OnlineResource> orig,
        OnlineEntity entity,
        OnlineEntity.EntityState state,
        OnlineResource resource)
    {
        orig(entity, state, resource);

        var online_player = entity as OnlineCreature;
        if (online_player == null) return;
        if (online_player.isMine) return;

        SyncBackSpears(online_player);
    }
}
