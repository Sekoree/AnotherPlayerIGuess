using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace APIG2.Models.Json;

public class TwitchRewardsResponse
{
    public record AutomaticReward(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("backgroundColor")] object BackgroundColor,
        [property: JsonPropertyName("cost")] object Cost,
        [property: JsonPropertyName("defaultBackgroundColor")] string DefaultBackgroundColor,
        [property: JsonPropertyName("defaultCost")] int? DefaultCost,
        [property: JsonPropertyName("defaultImage")] DefaultImage DefaultImage,
        [property: JsonPropertyName("image")] object Image,
        [property: JsonPropertyName("isEnabled")] bool? IsEnabled,
        [property: JsonPropertyName("isHiddenForSubs")] bool? IsHiddenForSubs,
        [property: JsonPropertyName("minimumCost")] int? MinimumCost,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("updatedForIndicatorAt")] string UpdatedForIndicatorAt,
        [property: JsonPropertyName("globallyUpdatedForIndicatorAt")] DateTime? GloballyUpdatedForIndicatorAt,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record Channel(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("self")] Self Self,
        [property: JsonPropertyName("__typename")] string Typename,
        [property: JsonPropertyName("communityPointsSettings")] CommunityPointsSettings CommunityPointsSettings
    );

    public record Community(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("displayName")] string DisplayName,
        [property: JsonPropertyName("channel")] Channel Channel,
        [property: JsonPropertyName("__typename")] string Typename,
        [property: JsonPropertyName("self")] object Self
    );

    public record CommunityPointsSettings(
        [property: JsonPropertyName("name")] object Name,
        [property: JsonPropertyName("image")] object Image,
        [property: JsonPropertyName("__typename")] string Typename,
        [property: JsonPropertyName("automaticRewards")] IReadOnlyList<AutomaticReward> AutomaticRewards,
        [property: JsonPropertyName("customRewards")] IReadOnlyList<CustomReward> CustomRewards,
        [property: JsonPropertyName("goals")] IReadOnlyList<object> Goals,
        [property: JsonPropertyName("isEnabled")] bool? IsEnabled,
        [property: JsonPropertyName("raidPointAmount")] int? RaidPointAmount,
        [property: JsonPropertyName("emoteVariants")] IReadOnlyList<EmoteVariant> EmoteVariants,
        [property: JsonPropertyName("earning")] Earning Earning
    );

    public record Data(
        [property: JsonPropertyName("community")] Community Community,
        [property: JsonPropertyName("currentUser")] object CurrentUser
    );

    public record DefaultImage(
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("url2x")] string Url2x,
        [property: JsonPropertyName("url4x")] string Url4x,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record Earning(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("averagePointsPerHour")] int? AveragePointsPerHour,
        [property: JsonPropertyName("cheerPoints")] int? CheerPoints,
        [property: JsonPropertyName("claimPoints")] int? ClaimPoints,
        [property: JsonPropertyName("followPoints")] int? FollowPoints,
        [property: JsonPropertyName("passiveWatchPoints")] int? PassiveWatchPoints,
        [property: JsonPropertyName("raidPoints")] int? RaidPoints,
        [property: JsonPropertyName("subscriptionGiftPoints")] int? SubscriptionGiftPoints,
        [property: JsonPropertyName("watchStreakPoints")] IReadOnlyList<WatchStreakPoint> WatchStreakPoints,
        [property: JsonPropertyName("multipliers")] IReadOnlyList<Multiplier> Multipliers,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record Emote(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("token")] string Token,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record EmoteVariant(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("isUnlockable")] bool? IsUnlockable,
        [property: JsonPropertyName("emote")] Emote Emote,
        [property: JsonPropertyName("modifications")] IReadOnlyList<Modification> Modifications,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record Extensions(
        [property: JsonPropertyName("durationMilliseconds")] int? DurationMilliseconds,
        [property: JsonPropertyName("operationName")] string OperationName,
        [property: JsonPropertyName("requestID")] string RequestID
    );

    public record GlobalCooldownSetting(
        [property: JsonPropertyName("isEnabled")] bool? IsEnabled,
        [property: JsonPropertyName("globalCooldownSeconds")] int? GlobalCooldownSeconds,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record Image(
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("url2x")] string Url2x,
        [property: JsonPropertyName("url4x")] string Url4x,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record MaxPerStreamSetting(
        [property: JsonPropertyName("isEnabled")] bool? IsEnabled,
        [property: JsonPropertyName("maxPerStream")] int? MaxPerStream,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record MaxPerUserPerStreamSetting(
        [property: JsonPropertyName("isEnabled")] bool? IsEnabled,
        [property: JsonPropertyName("maxPerUserPerStream")] int? MaxPerUserPerStream,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record Modification(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("emote")] Emote Emote,
        [property: JsonPropertyName("modifier")] Modifier Modifier,
        [property: JsonPropertyName("globallyUpdatedForIndicatorAt")] DateTime? GloballyUpdatedForIndicatorAt,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record Modifier(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record Multiplier(
        [property: JsonPropertyName("reasonCode")] string ReasonCode,
        [property: JsonPropertyName("factor")] double? Factor,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record Root(
        [property: JsonPropertyName("data")] Data Data,
        [property: JsonPropertyName("extensions")] Extensions Extensions
    );

    public record Self(
        [property: JsonPropertyName("communityPoints")] object CommunityPoints,
        [property: JsonPropertyName("__typename")] string Typename
    );

    public record WatchStreakPoint(
        [property: JsonPropertyName("points")] int? Points,
        [property: JsonPropertyName("__typename")] string Typename
    );
}

public record CustomReward(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("backgroundColor")] string BackgroundColor,
    [property: JsonPropertyName("cooldownExpiresAt")] object CooldownExpiresAt,
    [property: JsonPropertyName("cost")] int? Cost,
    [property: JsonPropertyName("defaultImage")] TwitchRewardsResponse.DefaultImage DefaultImage,
    [property: JsonPropertyName("image")] TwitchRewardsResponse.Image Image,
    [property: JsonPropertyName("maxPerStreamSetting")] TwitchRewardsResponse.MaxPerStreamSetting MaxPerStreamSetting,
    [property: JsonPropertyName("maxPerUserPerStreamSetting")] TwitchRewardsResponse.MaxPerUserPerStreamSetting MaxPerUserPerStreamSetting,
    [property: JsonPropertyName("globalCooldownSetting")] TwitchRewardsResponse.GlobalCooldownSetting GlobalCooldownSetting,
    [property: JsonPropertyName("isEnabled")] bool? IsEnabled,
    [property: JsonPropertyName("isInStock")] bool? IsInStock,
    [property: JsonPropertyName("isPaused")] bool? IsPaused,
    [property: JsonPropertyName("isSubOnly")] bool? IsSubOnly,
    [property: JsonPropertyName("isUserInputRequired")] bool? IsUserInputRequired,
    [property: JsonPropertyName("shouldRedemptionsSkipRequestQueue")] bool? ShouldRedemptionsSkipRequestQueue,
    [property: JsonPropertyName("redemptionsRedeemedCurrentStream")] object RedemptionsRedeemedCurrentStream,
    [property: JsonPropertyName("prompt")] string Prompt,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("updatedForIndicatorAt")] string UpdatedForIndicatorAt,
    [property: JsonPropertyName("__typename")] string Typename
);

//JsonContext SourceGenerator
[JsonSerializable(typeof(TwitchRewardsResponse.Root[]))]
public partial class TwichRewardsResponseContext : JsonSerializerContext
{
}
