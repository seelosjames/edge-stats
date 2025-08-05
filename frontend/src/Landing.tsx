import axios from "axios";
import { useEffect, useState } from "react";

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
	const [lines, setLines] = useState<Line[] | null>([]);
	const [leagues, setLeagues] = useState<string[]>([]);
	const [books, setBooks] = useState<string[]>([]);
	const [propTypes, setPropTypes] = useState<string[]>([]);

	const [league, setLeague] = useState<string | undefined>(undefined);
	const [sportsbook, setSportsbook] = useState<string | undefined>(undefined);
	const [propType, setPropType] = useState<string | undefined>(undefined);

	// useEffect(() => {
	// 	const fetchFilters = async () => {
	// 		const [sportsRes, booksRes, marketsRes] = await Promise.all([
	// 			fetch("https://localhost:7105/filters/sports"),
	// 			fetch("https://localhost:7105/filters/sportsbooks"),
	// 			fetch("https://localhost:7105/filters/prop-types"),
	// 		]);
	// 		setLeagues(await sportsRes.json());
	// 		setBooks(await booksRes.json());
	// 		setPropTypes(await marketsRes.json());
	// 	};

	// 	fetchFilters();
	// }, []);

	const [watchlistItems, setWatchlistItems] = useState<number[]>([]);

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

	// useEffect(() => {
	// 	const fetchLines = async () => {
	// 		const query = new URLSearchParams({
	// 			...(league && { league }),
	// 			...(sportsbook && { sportsbook }),
	// 			...(propType && { propType }),
	// 		}).toString();

	// 		const response = await fetch(`/lines?${query}`);
	// 		const data = await response.json();
	// 		setLines(data);
	// 	};

	// 	fetchLines();
	// }, [league, sportsbook, propType]);

	return (
		<main className="p-4 max-w-7xl mx-auto">
			{/* <section className="mb-6 bg-white p-4 rounded shadow">
				<h2 className="text-lg font-semibold mb-2">Filters</h2>
				<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
					<select value={league} onChange={(e) => setLeague(e.target.value)} className="w-full border p-2 rounded">
						<option value="">All Leagues</option>
						{leagues.map((sport, i) => (
							<option key={i} value={sport}>
								{sport}
							</option>
						))}
					</select>

					<select className="w-full border p-2 rounded">
						<option value="">All Books</option>
						{books.map((book, i) => (
							<option key={i} value={book}>
								{book}
							</option>
						))}
					</select>

					<select className="w-full border p-2 rounded">
						<option value="">Market Type</option>
						{propTypes.map((type, i) => (
							<option key={i} value={type}>
								{type}
							</option>
						))}
					</select>
					<input type="number" placeholder="Min Edge %" className="w-full border p-2 rounded" />
				</div>
			</section> */}

			<section className="bg-white p-4 rounded shadow">
				<h2 className="text-lg font-semibold mb-4">Top Value Bets</h2>
				<div className="overflow-x-auto">
					<table className="w-full text-sm text-left border">
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
					</table>
				</div>
			</section>
		</main>
	);
}

export default Landing;
