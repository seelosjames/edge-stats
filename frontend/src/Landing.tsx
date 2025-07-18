import { Link } from "react-router-dom";

function Landing() {
	return (
		<main className="p-4 max-w-7xl mx-auto">
			<section className="mb-6 bg-white p-4 rounded shadow">
				<h2 className="text-lg font-semibold mb-2">Filters</h2>
				<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
					<select className="w-full border p-2 rounded">
						<option value="">All Sports</option>
						<option>NBA</option>
						<option>NFL</option>
						<option>MLB</option>
					</select>
					<select className="w-full border p-2 rounded">
						<option value="">All Books</option>
						<option>FanDuel</option>
						<option>DraftKings</option>
						<option>Caesars</option>
					</select>
					<select className="w-full border p-2 rounded">
						<option value="">Market Type</option>
						<option>Moneyline</option>
						<option>Spread</option>
						<option>Over/Under</option>
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
								<th className="p-2">Sport</th>
								<th className="p-2">Matchup</th>
								<th className="p-2">Market</th>
								<th className="p-2">Book</th>
								<th className="p-2">Odds</th>
								<th className="p-2">Edge %</th>
								<th className="p-2">Starts In</th>
								<th className="p-2">Action</th>
							</tr>
						</thead>
						<tbody className="divide-y">
							<tr>
								<td className="p-2">NBA</td>
								<td className="p-2">Lakers vs Celtics</td>
								<td className="p-2">Moneyline</td>
								<td className="p-2">FanDuel</td>
								<td className="p-2">+125</td>
								<td className="p-2 text-green-600 font-semibold">+8.3%</td>
								<td className="p-2">3h 12m</td>
								<td className="p-2">
									<a href="#" className="text-blue-500 hover:underline">
										View
									</a>
								</td>
							</tr>
						</tbody>
					</table>
				</div>
			</section>
		</main>
	);
}

export default Landing;
