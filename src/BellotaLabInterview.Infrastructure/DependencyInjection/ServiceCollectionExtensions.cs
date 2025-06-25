using Microsoft.Extensions.DependencyInjection;
using BellotaLabInterview.Core.Domain.Cards;
using BellotaLabInterview.Core.Domain.Game;
using BellotaLabInterview.Uno.Cards;
using BellotaLabInterview.Uno.Effects;
using BellotaLabInterview.Uno.Game;
using BellotaLabInterview.Blackjack.Cards;
using BellotaLabInterview.Blackjack.Game;

namespace BellotaLabInterview.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBellotaLabServices(this IServiceCollection services)
        {
            return services
                .AddCoreServices()
                .AddUnoServices()
                .AddBlackjackServices()
                .AddHighStakesServices();
        }

        private static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            return services;
        }

        private static IServiceCollection AddHighStakesServices(this IServiceCollection services)
        {
            return services;
        }

        private static IServiceCollection AddUnoServices(this IServiceCollection services)
        {
            // Card components
            services.AddScoped<ICardFactory, UnoCardFactory>();
            services.AddScoped<IHandEvaluator, UnoHandEvaluator>();
            services.AddScoped<IDeck, UnoDeck>();
            
            // Game components
            services.AddScoped<IGameRules, UnoGameRules>();
            services.AddScoped<ICardEffectHandler, UnoEffectHandler>();
            services.AddScoped<IGame>(sp => new UnoGame(
                sp.GetRequiredService<IGameContext>(),
                sp.GetRequiredService<ICardFactory>(),
                sp.GetRequiredService<IHandEvaluator>(),
                sp.GetRequiredService<IDeck>(),
                new GameOptions { MinPlayers = 2, MaxPlayers = 10, InitialPoints = new Points(100) }
            ));

            return services;
        }

        private static IServiceCollection AddBlackjackServices(this IServiceCollection services)
        {
            // Card components
            services.AddScoped<ICardFactory, BlackjackCardFactory>();
            services.AddScoped<IHandEvaluator, BlackjackHandEvaluator>();
            services.AddScoped<IDeck, BlackjackDeck>();

            // Game components
            services.AddScoped<IGameRules, BlackjackGameRules>();
            services.AddScoped<IGame>(sp => new BlackjackGame(
                sp.GetRequiredService<IGameContext>(),
                sp.GetRequiredService<ICardFactory>(),
                sp.GetRequiredService<IHandEvaluator>(),
                sp.GetRequiredService<IDeck>(),
                sp.GetRequiredService<IGameRules>()
            ));

            return services;
        }
    }
} 