#include <iostream>
#include <string>
#include <bitset>
#include <vector>
#include <algorithm>

using namespace std;
using uint = unsigned int;

constexpr int store_capacity = 4;

class SlimArray {
public:
	explicit SlimArray(size_t cell_count, size_t cell_capacity) {
	}

	char get(size_t index) const {
		return bitset.test(index);
	}

	void set(size_t index, char value) {
		bitset.set(index, value > 0);
	}

private:
	bitset<1 << 22> bitset;
};

class HashCounter {
public:
	explicit HashCounter(const int shift):	shift_size(shift), ranks(SlimArray(1 << shift_size, store_capacity)) {
	}

	void add(const uint hash) {
		auto index = hash % (1 << shift_size);
		auto rank = calc_rank((hash >> shift_size) % (1 << (8 / store_capacity))) + 1;

		if (ranks.get(index) < rank) {
			ranks.set(index, rank);
		}	
	}

	int count() const {
		auto alpha = 0.7213 / (1 + 1.079 / shift_size);
		auto size = 1 << shift_size;

		auto sum = 0.0;

		for (auto i = 0; i < size; i++) {
			sum += 1 / pow(2.0, ranks.get(i));
		}

		auto result = alpha * size * size / sum;

		if (result > pow(2.0, 32) / 30.0)
		{
			return -pow(2.0, 32.0) * log(1.0 - result / pow(2.0, 32.0));
		}
		
		if (result <= 2.5 * size)
		{
			auto v = 0;
			
			for (auto i = 0; i < size; ++i) {
				if (ranks.get(i) == 0) ++v;
			}
			
			if (v > 0) {
				return size * log((double) size / v);
			}

			return result;
		}

		return result;
	}
	
private:
	uint shift_size;
	SlimArray ranks;
	
	static uint calc_rank(uint hash)
	{
		uint rank = 0;

		while ((hash ^ 0) & 1)
		{
			rank++;
			hash >>= 1;
		}

		return rank;
	}	
};

uint calc_hash(const string& s) {
	return hash<string>{}(s);
}

uint fnv1a(const string& s)
{
	auto hash = 2166136261U;

	for (auto i = 0; i < s.length(); ++i)
	{
		hash ^= s[i];
		hash += (hash << 1) + (hash << 4) + (hash << 7) + (hash << 8) + (hash << 24);
	}
	
	return hash;
}

int main() {
	string s;
	HashCounter counter(22);

	while (cin >> s) {
		auto hash = calc_hash(s);
		
		counter.add(hash);
	}

	cout << counter.count();
	
	return 0;
}
