import axios from "axios";
import { useEffect, useState } from "react";

type Line = {
	lineId: number;
	lineUuid: string;
	propId: number;
	prop: Prop;
	sportsbookId: number;
	sportsbook: Sportsbook;
	description: string;
	odd: number;
};

type Prop = {
	propId: number;
	propUuid: string;
	gameId: number;
	propName: string;
	propType: string;
	game: Game;
};

type Sportsbook = {
	sportsbookId: number;
	sportsbookName: string;
};

type Game = {
	gameId: number;
	gameUuid: string;
	leagueId: number;
	league: League;
	team1: Team;
	team2: Team;
	gameDateTime: Date;
};

type Team = {
	teamId: number;
	teamName: string;
	leagueId: number;
	league: League;
};

type League = {
	leagueId: number;
	leagueName: string;
	sportType: string;
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

	useEffect(() => {
		const fetchFilters = async () => {
			const [sportsRes, booksRes, marketsRes] = await Promise.all([
				fetch("https://localhost:7105/filters/sports"),
				fetch("https://localhost:7105/filters/sportsbooks"),
				fetch("https://localhost:7105/filters/prop-types"),
			]);
			setLeagues(await sportsRes.json());
			setBooks(await booksRes.json());
			setPropTypes(await marketsRes.json());
		};

		fetchFilters();
	}, []);

	useEffect(() => {
		axios
			.get<Line[]>("https://localhost:7105/lines")
			.then(function (response) {
				setLines(response.data);
			})
			.catch(function (error) {
				console.error("Error fetching data:", error);
			});
	}, []);

	useEffect(() => {
		const fetchLines = async () => {
			const query = new URLSearchParams({
				...(league && { league }),
				...(sportsbook && { sportsbook }),
				...(propType && { propType }),
			}).toString();

			const response = await fetch(`/lines?${query}`);
			const data = await response.json();
			setLines(data);
		};

		fetchLines();
	}, [league, sportsbook, propType]);

	return (
		<main className="p-4 max-w-7xl mx-auto">
			<section className="mb-6 bg-white p-4 rounded shadow">
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
			</section>

			<section className="bg-white p-4 rounded shadow">
				<h2 className="text-lg font-semibold mb-4">Top Value Bets</h2>
				<div className="overflow-x-auto">
					<table className="w-full text-sm text-left border">
						<thead className="bg-gray-200">
							<tr>
								<th className="p-2">League</th>
								<th className="p-2">Matchup</th>
								<th className="p-2">Sportsbook</th>
								{/* <th className="p-2">Prop Name</th> */}
								<th className="p-2">Prop Type</th>
								<th className="p-2">Description</th>
								<th className="p-2">Odds</th>
								<th className="p-2">Starts In</th>
							</tr>
						</thead>
						<tbody className="divide-y">
							{lines?.map((line) => (
								<tr key={line.lineId}>
									<td className="p-2">{line.prop.game.league.leagueName}</td>
									<td className="p-2">
										{line.prop.game.team1.teamName} vs. {line.prop.game.team2.teamName}
									</td>
									<td className="p-2">{line.sportsbook.sportsbookName}</td>

									{/* <td className="p-2">{line.prop.propName}</td> */}
									<td className="p-2">{line.prop.propType}</td>
									<td className="p-2">{line.description}</td>
									<td className="p-2">{line.odd}</td>
									<td className="p-2">{getStartsInString(line.prop.game.gameDateTime)}</td>
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
