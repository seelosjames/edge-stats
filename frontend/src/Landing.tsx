import axios from "axios";
import { useEffect, useRef, useState } from "react";
import { FaFilter } from "react-icons/fa";

type Line = {
	lineId: number;
	propName: string;
	propType: string;
	sportsbookName: string;
	description: string;
	odd: number;
	team1: string;
	team2: string;
	gameDateTime: Date;
	gameStatus: string | null;
};

type WatchlistItem = {
	lineId: number;
};

// utils/dateHelpers.js
export function getStartsInString(dateTimeStr: Date) {
	const gameTime = new Date(dateTimeStr);
	const now = new Date();
	const diffMs = gameTime.getTime() - now.getTime();

	if (diffMs <= 0) return "Already started";

	const diffMins = Math.floor(diffMs / 60000);
	const diffHours = Math.floor(diffMins / 60);
	const diffDays = Math.floor(diffHours / 24);

	if (diffDays > 0) return `Starts in ${diffDays} day${diffDays > 1 ? "s" : ""}`;
	if (diffHours > 0) return `Starts in ${diffHours} hour${diffHours > 1 ? "s" : ""}`;
	return `Starts in ${diffMins} minute${diffMins > 1 ? "s" : ""}`;
}

function Landing() {
	const [showFilters, setShowFilters] = useState(false);
	const [selectedLeagues, setSelectedLeagues] = useState<string[]>([]);
	const [selectedBooks, setSelectedBooks] = useState<string[]>([]);
	const filterRef = useRef<HTMLDivElement>(null);
	const [lines, setLines] = useState<Line[] | null>([]);
	const [watchlistItems, setWatchlistItems] = useState<number[]>([]);

	const handleRefreshOdds = async () => {
		try {
			const response = await axios.post("https://localhost:7105/scraper", {
				Leagues: selectedLeagues,
				Sportsbooks: selectedBooks,
			});

			const { recordsSaved, sourceCount, sportCount } = response.data;

			console.log(`Scraped ${recordsSaved} odds from ${sourceCount} sportsbooks across ${sportCount} sports.`);
		} catch (error) {
			console.error("Error scraping odds:", error);
		}
	};

	// Click outside handler
	useEffect(() => {
		function handleClickOutside(event: MouseEvent) {
			if (filterRef.current && !filterRef.current.contains(event.target as Node)) {
				setShowFilters(false);
			}
		}

		document.addEventListener("mousedown", handleClickOutside);
		return () => {
			document.removeEventListener("mousedown", handleClickOutside);
		};
	}, [filterRef]);

	useEffect(() => {
		const fetchWatchlist = async () => {
			try {
				const response = await axios.get<WatchlistItem[]>("https://localhost:7105/watchlist");
				setWatchlistItems(response.data.map((item) => item.lineId));
			} catch (error) {
				console.error("Error fetching watchlist:", error);
			}
		};

		fetchWatchlist();
	}, []);

	const handleStarClick = async (lineId: number): Promise<void> => {
		try {
			if (watchlistItems.includes(lineId)) {
				await axios.delete(`https://localhost:7105/watchlist/${lineId}`);
				setWatchlistItems((prev) => prev.filter((id) => id !== lineId));
			} else {
				const response = await axios.post<WatchlistItem>("https://localhost:7105/watchlist", {
					lineId,
				});
				setWatchlistItems((prev) => [...prev, response.data.lineId]);
			}
		} catch (error) {
			console.error("Error updating watchlist:", error);
		}
	};

	useEffect(() => {
		const fetchLines = async () => {
			try {
				const response = await axios.get<Line[]>("https://localhost:7105/lines");
				setLines(response.data);
			} catch (error) {
				console.error("Error fetching lines:", error);
			}
		};

		fetchLines();
	}, []);

	return (
		<section className="p-4 max-w-7xl mx-auto flex-grow">
			<div className="flex justify-between">
				<h2 className="text-lg font-semibold mb-4">Top Value Bets</h2>

				<div className="flex items-center gap-2">
					<p className="text-sm text-jet-500">
						Last updated: <span id="last-updated">2 mins ago</span>
					</p>

					<button className="bg-sage-500 text-black px-4 py-2 rounded hover:bg-sage-400" onClick={handleRefreshOdds}>
						Refresh Odds
					</button>
					
				</div>
			</div>
			<div className="overflow-x-auto">
				<table className="min-w-full table-auto border border-gray-300 text-sm text-left shadow-md rounded-lg overflow-hidden">
					<thead className="bg-gray-100 text-gray-700 uppercase text-xs font-semibold">
						<tr>
							<th className="px-4 py-2 text-center"></th>
							<th className="px-4 py-2">Matchup</th>
							<th className="px-4 py-2">League</th>
							<th className="px-4 py-2">Market</th>
							<th className="px-4 py-2">Pick</th>
							<th className="px-4 py-2">Book</th>
							<th className="px-4 py-2">Odds</th>
							<th className="px-4 py-2">Edge</th>
						</tr>
					</thead>
					<tbody className="divide-y divide-gray-200 bg-white">
						<tr key="{0}" className="hover:bg-gray-50">
							<td className="px-4 py-2 text-center">★</td>
							<td className="px-4 py-2">Colts vs. Patriots</td>
							<td className="px-4 py-2">NFL</td>
							<td className="px-4 py-2">Game Moneyline</td>
							<td className="px-4 py-2">Colts</td>
							<td className="px-4 py-2">Fliff</td>
							<td className="px-4 py-2">+110</td>
							<td className="px-4 py-2 text-green-600 font-medium">+4.2%</td>
						</tr>
						<tr key="{1}" className="hover:bg-gray-50">
							<td className="px-4 py-2 text-center">★</td>
							<td className="px-4 py-2">Jaguars vs. Titans</td>
							<td className="px-4 py-2">NFL</td>
							<td className="px-4 py-2">Trevor Lawrence pass yds</td>
							<td className="px-4 py-2">Over 225.5</td>
							<td className="px-4 py-2">Underdog</td>
							<td className="px-4 py-2">-115</td>
							<td className="px-4 py-2 text-green-600 font-medium">+2.8%</td>
						</tr>
						<tr key="{2}" className="hover:bg-gray-50">
							<td className="px-4 py-2 text-center">★</td>
							<td className="px-4 py-2">Stanford vs. Hawaii</td>
							<td className="px-4 py-2">NCAAF</td>
							<td className="px-4 py-2">Game Spread</td>
							<td className="px-4 py-2">Stanford -1.5</td>
							<td className="px-4 py-2">Fliff</td>
							<td className="px-4 py-2">-120</td>
							<td className="px-4 py-2 text-green-600 font-medium">+3.5%</td>
						</tr>
						<tr key="{3}" className="hover:bg-gray-50">
							<td className="px-4 py-2 text-center">★</td>
							<td className="px-4 py-2">49ers vs. Packers</td>
							<td className="px-4 py-2">NFL</td>
							<td className="px-4 py-2">First Quarter Total</td>
							<td className="px-4 py-2">Over 9.5</td>
							<td className="px-4 py-2">Prize Picks</td>
							<td className="px-4 py-2">+105</td>
							<td className="px-4 py-2 text-green-600 font-medium">+1.9%</td>
						</tr>
					</tbody>
				</table>
				{/* <table className="w-full text-sm text-left border">
					<thead className="bg-gray-200">
						<tr>
							<th className="p-2">Line ID</th>
							<th className="p-2">Matchup</th>
							<th className="p-2">Sportsbook</th>
							<th className="p-2">Prop Name</th>
							<th className="p-2">Prop Type</th>
							<th className="p-2">Description</th>
							<th className="p-2">Odds</th>
							<th className="p-2">Starts In</th>
							<th className="p-2"></th>
						</tr>
					</thead>
					<tbody className="divide-y">
						{lines?.map((line) => (
							<tr key={line.lineId}>
								<td className="p-2">{line.lineId}</td>
								<td className="p-2">
									{line.team1} vs. {line.team2}
								</td>
								<td className="p-2">{line.sportsbookName}</td>
								<td className="p-2">{line.propName}</td>
								<td className="p-2">{line.propType}</td>
								<td className="p-2">{line.description}</td>
								<td className="p-2">{line.odd}</td>
								<td className="p-2">{getStartsInString(line.gameDateTime)}</td>
								<td className="p-2 text-center">
									<button
										onClick={() => handleStarClick(line.lineId)}
										className={`text-2xl ${
											watchlistItems.includes(line.lineId) ? "text-yellow-500" : "text-gray-300 hover:text-yellow-500"
										}`}
										aria-label="Favorite"
									>
										★
									</button>
								</td>
							</tr>
						))}
					</tbody>
				</table> */}
			</div>
		</section>
	);
}

export default Landing;
